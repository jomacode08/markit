import { FormArray, FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { catchError, debounceTime, delay, Observable, of, retry, RetryConfig, Subject, Subscription, switchMap, takeUntil, tap, timer } from 'rxjs';
import { CommonModule } from '@angular/common';
import { Component, signal, OnInit, OnDestroy, inject, ChangeDetectionStrategy, ViewChild } from '@angular/core';

import { ButtonModule } from 'primeng/button';
import { DialogService, DynamicDialogRef } from 'primeng/dynamicdialog';
import { Editor } from '@tiptap/core';
import { MessageService } from 'primeng/api';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { SkeletonModule } from 'primeng/skeleton';
import { TooltipModule } from 'primeng/tooltip';

import { Block } from './../../interfaces/block';
import { BlockComponent } from '../../components/block/block.component';
import { BlockMenuComponent, OnCloseResponse } from '../../components/block-menu/block-menu.component';
import { BlockNavigatorComponent } from '../../components/block-navigator/block-navigator.component';
import { BlockService } from '../../services/block.service';
import { CanComponentDeactivate, CanDeactivateType } from '../../../auth/guards/can-deactivate/can-component-deactivate';
import { createTextFormattingOptions } from '../../interfaces/text-formatting-options';
import { CurrentRouteService } from '../../../shared/services/current-route.service';
import { CustomMessageService } from '../../../shared/services/custom-message.service';
import { EmojiPickerComponent } from '../../../shared/components/ui/emoji-picker/emoji-picker.component';
import { FloatingActionButtonComponent } from '../../../shared/components/ui/buttons/floating-action-button/floating-action-button.component';
import { FloatingMenuComponent } from '../../../shared/components/layout/floating-menu/floating-menu.component';
import { FloatingMenuOption } from '../../../shared/components/layout/floating-menu/floating-menu-option';
import { Mark } from '../../interfaces/mark';
import { MARK_VIEWER_CONSTANTS } from './constants/mark-viewer-constants';
import { MarkAutosaveIndicatorComponent, SaveState } from '../../components/mark-autosave-indicator/mark-autosave-indicator.component';
import { MarkBreadcrumbComponent } from '../../components/mark-breadcrumb/mark-breadcrumb.component';
import { MarkService } from '../../services/mark.service';
import { ROUTES } from '../../../shared/utils/constant';
import { SharedData } from './../../components/block-menu/block-menu.component';

@Component({
    imports: [
        BlockComponent,
        BlockNavigatorComponent,
        ButtonModule,
        CommonModule,
        EmojiPickerComponent,
        FloatingActionButtonComponent,
        FloatingMenuComponent,
        MarkAutosaveIndicatorComponent,
        MarkBreadcrumbComponent,
        ProgressSpinnerModule,
        ReactiveFormsModule,
        SkeletonModule,
        TooltipModule,
    ],
    providers: [DialogService],
    templateUrl: './mark-viewer.component.html',
    styleUrl: './mark-viewer.component.css',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class MarkViewerComponent implements OnInit, OnDestroy, CanComponentDeactivate {
  //* Configuration
  @ViewChild('floatingMenu') private floatingMenu !: FloatingMenuComponent;
  private blockMenuDialogRef ?: DynamicDialogRef<BlockMenuComponent> | null;
  private destroy$ = new Subject<void>();
  private fb = inject(FormBuilder);

  //* Form
  public markForm = this.fb.nonNullable.group({
    id: [0],
    inputName: ['My new mark 🎉', [Validators.required, Validators.maxLength(255)]],
    emoji: [undefined as string | undefined],
    blocks : this.fb.array<FormGroup>([]),
    // Non-editable properties
    name: [''],
    collectionId: [0],
    creatorId: [0],
    collectionName: [''],
    requiresSync: [false]
  });
  private editor = signal<Editor | undefined>(undefined)
  public saveState  = signal<SaveState>(SaveState.idle);

  //* State management
  protected mark$ : Observable<Mark | null>;
  private markNameInputDebouncer = new Subject<string>();
  public currentMark = signal<Mark>(this.markForm.getRawValue() as Mark);
  public currentBlock = signal<Block|undefined>(undefined);
  public currentBlockIndex = signal<number>(0);

  //* Floating menu
  public floatingMenuOptions = signal<FloatingMenuOption[]>([]);

  //* Getters
  get markNamePlaceHolder(): string {
    return MARK_VIEWER_CONSTANTS.MARK_NAME_PLACEHOLDER;
  }
  get notFoundPlaceHolder(): string {
    return MARK_VIEWER_CONSTANTS.NOT_FOUND_PLACEHOLDER;
  }

  //* Lyfecycle
  constructor(
    private activatedRoute: ActivatedRoute,
    private blockService: BlockService,
    private currentRouteService: CurrentRouteService,
    private dialogService: DialogService,
    private markService: MarkService,
    private messageService: CustomMessageService,
    private router: Router,
    private toastService: MessageService,
  ) {
    this.mark$ = this.activatedRoute.params
    .pipe(
      // Get the markId param and fetch mark data
      switchMap((params) => {
        const markId = this.getValidMarkId(params.id);
        return this.fetchMark(markId);
      }),
      // Initialize form and check mark sync.
      tap(mark => {
        this.initializeForm(mark);
        if (mark.requiresSync) this.updateMarkWithRetry();
      }),
      catchError(error => {
        this.redirectOnError();
        return of(null);
      })
    );
  }
  
  public async ngOnInit(): Promise<void> {
    this.floatingMenuOptions.set(createTextFormattingOptions(this.editor));
    this.subscribeToFormChanges();
    this.subscribeToEmojiChanges();
    this.subscribeToDebouncedMarkNameInput();
  }

  public ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  public canDeactivate(): CanDeactivateType {
    if (this.saveState() === SaveState.saving) {
      const deactivateSubject = new Subject<boolean>();

      this.openUnsavedChangesDialog(() => {
        deactivateSubject.next(true);
        deactivateSubject.complete();
      });
      return deactivateSubject;
    }
    return true;
  }

  //* Events
  public onEditorSelected = (editor : Editor) => this.editor.set(editor);

  public onEditorValueChanged(content: string) {
    const block = this.currentBlock();
    if (block) {
      this.updateBlockContentWithRetry(block.id, content);
    }
  }

  public onCancel(): void {
    this.redirectToUrl(this.currentRouteService.previousSuccessfulUrl() ?? ROUTES.MY_MARKS);
  }

  public onTextFormattingButtonClick(): void {
    this.changeFloatingMenuState();
  }

  public retryMarkSave(): void {
    this.updateMarkWithRetry();
  }

  public onMarkNameInputChanged(event: Event) {
    let name = (event.target as HTMLInputElement).value;
    this.markNameInputDebouncer.next(name);
  }

  public openBlockMenuDialog( blocks : Block[] ) {
    this.blockMenuDialogRef = this.dialogService.open(BlockMenuComponent, {
      header: 'Blocks',
      width : '30rem',
      modal : true,
      closable : true,
      dismissableMask : true,
      styleClass : 'custom-dialog',
      data: {
        shared : {
          blocks: structuredClone(blocks),
          currentBlockId: this.currentBlock()?.id
        } as SharedData
      }
    });

    this.blockMenuDialogRef?.onClose
    .subscribe(async ( response: OnCloseResponse ) => 
      await this.handleOnCloseBlockMenu(response)
    );
  }

  private async handleOnCloseBlockMenu(response: OnCloseResponse): Promise<void> {
    // Clean dialog reference
    this.blockMenuDialogRef = null;
    if (response == null) return;
    // A new block was selected
    if (response.selectedBlockId != null) {
      const currentMark = this.currentMark();
      const selectedBlockIndex = currentMark.blocks.findIndex(b => b.id === response.selectedBlockId);
      this.modifyActiveBlock(selectedBlockIndex);
      return;
    }
    // Apply block changes
    if (response.blocks) {
      await this.setBlocks(response.blocks);
      this.updateMarkWithRetry();
      this.modifyActiveBlock(this.currentBlockIndex());
    }
  }

  public openUnsavedChangesDialog( accept: () => void ): void {
    this.messageService.showConfirmationDialog({
      message: 'Are you sure you want to leave? Some data may be lost.',
      header: 'Unsaved changes',
      icon: 'fa fa-warning',
      accept
    });
  }

  //* Form
  public getCurrentBlockFormGroup() {
    const blocks = this.markForm.get('blocks') as FormArray;
    return blocks.at(this.currentBlockIndex()) as FormGroup;
  }
  private initializeForm(mark: Mark): void {
    this.markForm.reset(mark, {emitEvent: false});
    this.setBlocks(mark.blocks);
    this.modifyActiveBlock(0);
  }

  private subscribeToFormChanges(): Subscription {
    return this.markForm.valueChanges
    .pipe(takeUntil(this.destroy$))
    .subscribe(changes => {
      this.currentMark.update(current => changes as Mark);
    });
  }
  
  private subscribeToEmojiChanges(): Subscription {
    return this.markForm.controls.emoji.valueChanges
    .pipe(takeUntil(this.destroy$))
    .subscribe(emoji => {
      if (this.currentMark().emoji != emoji) {
        this.updateMarkWithRetry();
      }
    });
  }

  private redirectOnError(): void {
    this.redirectToUrl(ROUTES.ERROR);
  }

  //* Marks
  private fetchMark(markId : number): Observable<Mark> {
    const localMark = this.markService.getMarkFromLocalStorage(markId);
    if (localMark) return of(localMark);
    return this.getMarkById(markId);
  }

  private getMarkById( id: number ): Observable<Mark> {
    return this.markService.getById(id);
  }

  private updateMarkSyncStatus(state: boolean): void {
    this.markForm.value.requiresSync = state;
  }

  private resetMarkSync(markId: number): void {
    this.updateMarkSyncStatus(false);
    this.markService.dropMarkFromLocalStorage(markId);
  }

  private updateMarkWithRetry(): void {
    this.setSaveState(SaveState.saving);
    const currentMark = this.markForm.getRawValue() as Mark;
    this.markService.update(currentMark)
    .pipe(
      // Error retry with exponential backoff
      retry(this.getErrorRetryConfig()),
      delay(500),
    )
    .subscribe({
      next:  (mark)  => {
        this.markForm.reset(mark);
        this.setSaveState(SaveState.saved);
        if (currentMark.requiresSync)
          this.resetMarkSync(currentMark.id);
      },
      // After maximum retries, set error state and buffer unsaved data.
      error: ()  => this.saveChangesLocally()
    });
  }

  private subscribeToDebouncedMarkNameInput(): Subscription {
    const DEBOUNCE_TIME_IN_MILLI_SECONDS = 1000;
    return this.markNameInputDebouncer
    .pipe(
      takeUntil(this.destroy$),
      debounceTime(DEBOUNCE_TIME_IN_MILLI_SECONDS)
    )
    .subscribe((name) => {
      this.markForm.value.name = name;
      this.updateMarkWithRetry();
    });
  }

  //* Blocks
  private async setBlocks(blocks: Block[]): Promise<void> {
    const blockControls = blocks.map(block =>
      this.fb.group({
        id: [block.id, Validators.required],
        content: [block.content, Validators.required],
        title: [block.title, Validators.required],
        createdDate: [block.createdDate],
      })
    );

    this.markForm.controls.blocks.clear();
    blockControls.forEach(c => this.markForm.controls.blocks.push(c));
  }

  private updateBlockContentWithRetry( id:number, content: string ): void {
    this.setSaveState(SaveState.saving);

    this.blockService.updateContent(id, content).pipe(
      // Error retry with exponential backoff
      retry(this.getErrorRetryConfig()),
      delay(500),
    ).subscribe({
      next: () => this.setSaveState(SaveState.saved),
      // After maximum retries, set error state and buffer unsaved data.
      error: (error) => this.saveChangesLocally(),
    });
  }

  //* UTILS
  private setSaveState = (state: SaveState) => this.saveState.set(state);
  private redirectToUrl = (url: string) => this.router.navigate([url]);
  
  public modifyActiveBlock(newIndex: number) {
    const currentMark = this.currentMark();
    if (newIndex < 0 || newIndex >= currentMark.blocks.length) return;
    this.currentBlockIndex.update(c => newIndex);
    this.currentBlock.update(c => currentMark.blocks[newIndex]);
  }

  private getValidMarkId(markIdParam: string): number {
    if (markIdParam == null || isNaN(Number(markIdParam))) {
      throw new Error("The markId is invalid");
    }
    return Number(markIdParam);
  }

  private changeFloatingMenuState(): void {
    this.floatingMenu.toggle();
  }

  private getErrorRetryConfig(): RetryConfig {
    const { maxEntries, delayInMs } = MARK_VIEWER_CONSTANTS.ERROR_RETRY_SETTINGS;
    return {
      count: maxEntries,
      delay : (error, retryCount) => {
        this.toastService.clear();
        const baseDelay = Math.pow(2, retryCount) * delayInMs;
        const jitter = Math.random() * 1000;
        return timer(baseDelay + jitter)
      },
    };
  }

  private saveChangesLocally(): void {
    this.setSaveState(SaveState.error);
    this.updateMarkSyncStatus(true);
    this.markService.setMarkInLocalStorage(this.currentMark());
  }
}