import { AbstractControl, FormArray, FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Component, signal, OnInit, computed, OnDestroy } from '@angular/core';
import { debounceTime, delay, Observable, of, retry, RetryConfig, Subject, Subscription, throwError, timer } from 'rxjs';
import { CommonModule } from '@angular/common';

import { ButtonModule } from 'primeng/button';
import { GalleriaModule } from 'primeng/galleria';
import { ChipModule } from 'primeng/chip';
import { Editor } from '@tiptap/core';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { SkeletonModule } from 'primeng/skeleton';
import { TooltipModule } from 'primeng/tooltip';
import { MessageService } from 'primeng/api';

import { AuthService } from '../../../auth/services/auth.service';
import { Block } from '../../interfaces/block';
import { BlockComponent } from '../../components/block/block.component';
import { BlockMenuComponent, OnCloseResponse } from '../../components/block-menu/block-menu.component';
import { BlockService } from '../../services/block.service';
import { CanComponentDeactivate, CanDeactivateType } from '../../../auth/guards/can-deactivate/can-component-deactivate';
import { createTextFormattingOptions } from '../../interfaces/text-formatting-options';
import { CurrentRouteService } from '../../../shared/services/current-route.service';
import { CustomMessageService } from '../../../shared/services/custom-message.service';
import { DEFAULT_BLOCK_NAME } from '../../../shared/utils/constant';
import { DEFAULT_MARK_NAME, ROUTES } from '../../../shared/utils/constant';
import { DialogService, DynamicDialogRef } from 'primeng/dynamicdialog';
import { EmojiPickerComponent } from '../../../shared/components/ui/emoji-picker/emoji-picker.component';
import { FloatingActionButtonComponent } from '../../../shared/components/ui/buttons/floating-action-button/floating-action-button.component';
import { FloatingMenuComponent } from '../../../shared/components/layout/floating-menu/floating-menu.component';
import { FloatingMenuOption } from '../../../shared/components/layout/floating-menu/floating-menu-option';
import { Mark } from '../../interfaces/mark';
import { MarkService } from '../../services/mark.service';
import { SharedData } from './../../components/block-menu/block-menu.component';

enum SaveState {
  idle,
  saving,
  saved,
  error
}

@Component({
  standalone: true,
  imports: [
    BlockComponent,
    ButtonModule,
    ChipModule,
    EmojiPickerComponent,
    GalleriaModule,
    CommonModule,
    FloatingActionButtonComponent,
    FloatingMenuComponent,
    ProgressSpinnerModule,
    ReactiveFormsModule,
    SkeletonModule,
    TooltipModule,
  ],
  providers: [DialogService],
  templateUrl: './mark-viewer.component.html',
  styleUrl: './mark-viewer.component.css',
})
export class MarkViewerComponent implements OnInit, OnDestroy, CanComponentDeactivate {
  //* Configuration
  private blockMenuDialogRef: DynamicDialogRef | undefined;
  private editor = signal<Editor | undefined>(undefined);
  // Constants
  public readonly MARK_NAME_PLACEHOLDER : string = 'New mark';
  private readonly MARKID_PARAM_NAME : string = 'id';
  private readonly SEE_MARK_ROUTE : string = 'marks/see';
  private readonly ERROR_RETRY_SETTINGS = {
    maxEntries: 3,
    delayInMs: 1000
  }
  // Routing
  private previousUrl : string | null = null;
  private currentUrl : string  | null = null;
  // Floating menu
  public floatingMenuOptions : FloatingMenuOption[] = [];
  public isFloatingMenuVisible : boolean = false;
  public textFormattingOptions: FloatingMenuOption[] = [];
  // Block gallery  
  public currentBlockIndex = signal<number>(0);
  
  //* Form
  private markNameInputDebouncer = new Subject<string>();
  private markNameInputDebounceSub ?: Subscription;
  public saveState  = signal<SaveState>(SaveState.idle);
  public creatorName ?: string;
  public form = new FormGroup({
    id       : new FormControl<number>(0),
    name : new FormControl<string>(""),
    inputName     : new FormControl<string>("My new mark 🎉", [Validators.required, Validators.maxLength(255)]),
    collectionId : new FormControl<number>(0),
    collectionName : new FormControl<string>(''),
    emoji : new FormControl<string|undefined>(undefined),
    requiresSync : new FormControl<boolean>(false),
    blocks   : new FormArray<FormGroup>([]),
  });

  //* Getters
  get currentMark(): Mark {
    return this.form.value as Mark;
  }
  get currentBlock(): Block {
    return this.currentBlocks.at(this.currentBlockIndex()).value as Block;
  }
  get currentBlocks() {
    return this.form.get('blocks') as FormArray;
  }
  get saveStateType(): typeof SaveState {
    return SaveState;
  }

  //* Lyfecycle
  constructor(
    private activatedRoute: ActivatedRoute,
    private authService: AuthService,
    private blockService: BlockService,
    private currentRouteService: CurrentRouteService,
    private dialogService: DialogService,
    private fb: FormBuilder,
    private markService: MarkService,
    private messageService: CustomMessageService,
    private router: Router,
    private toastService: MessageService,
  ) {
    this.previousUrl = this.currentRouteService.previousSuccessfulUrl();
    this.currentUrl = this.currentRouteService.url();
    this.textFormattingOptions = createTextFormattingOptions(this.editor);
    this.creatorName = this.authService.currentUser()?.given_name;
  }

  public async ngOnInit(): Promise<void> {
    this.fetchMark(this.currentUrl ?? '').subscribe({
      next: (mark) => {
        this.initializeForm(mark);
        if (mark.requiresSync) this.updateMarkWithRetry(mark);
      },
      error: (error) => this.handleError(error)
    });

    this.markNameInputDebounceSub = this.handleMarkNameInputDebounce();
  }

  public ngOnDestroy(): void {
    if (this.markNameInputDebounceSub) {
      this.markNameInputDebounceSub.unsubscribe();
    }
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
    this.updateBlockContentWithRetry(this.currentBlock.id, content);
  }

  public onCancel(): void {
    this.redirectToUrl(this.previousUrl ?? ROUTES.MY_MARKS);
  }

  public onTextFormattingButtonClick( ): void {
    this.changeFloatingMenuState(this.textFormattingOptions);
  }

  public onErrorSavingButtonClick(): void {
    this.updateMarkWithRetry(this.currentMark);
  }

  public onCollectionBtnClick(): void {
    this.redirectToUrl(ROUTES.COLLECTION_SEE(this.currentMark.collectionId));
  }

  public onEmojiSelected(emoji: string) {
    this.currentMark.emoji = emoji;
    this.updateMarkWithRetry(this.currentMark);
  } 
  
  public onEmojiDeleted() {
    this.currentMark.emoji = undefined;
    this.updateMarkWithRetry(this.currentMark);
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
      closable : false,
      dismissableMask : true,
      styleClass : 'custom-dialog',
      data: {
        shared : {
          blocks: structuredClone(blocks),
          currentBlockId: this.currentBlock.id
        } as SharedData
      }
    });

    this.blockMenuDialogRef.onClose
    .subscribe(async ( response: OnCloseResponse ) => 
      await this.handleOnCloseBlockMenu(response)
    );
  }

  private async handleOnCloseBlockMenu(response: OnCloseResponse): Promise<void> {
    // Clean dialog reference
    this.blockMenuDialogRef = undefined;
    if (response == null) return;
    // A new block was selected
    if (response.selectedBlockId != null) {
      const selectedBlockIndex = this.currentMark.blocks.findIndex(b => b.id === response.selectedBlockId);
      this.currentBlockIndex.set(selectedBlockIndex);
      return;
    }
    // Apply block changes
    if (response.blocks) {
      await this.setBlocks(response.blocks);
      this.updateMarkWithRetry(this.currentMark);
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

  public navigateForward(): void {
    if (this.currentBlockIndex() === this.currentBlocks.length - 1) return;
    this.currentBlockIndex.update(current => current + 1);
  }

  public navigateBackward(): void {
    if (this.currentBlockIndex() === 0) return;
    this.currentBlockIndex.update(current => current - 1);
  }

  //* Form
  private initializeForm(mark: Mark): void {
    this.setBlocks(mark.blocks);
    this.form.reset(mark);
  }

  private handleError(error: Error): Observable<null> {
    this.redirectToUrl(this.previousUrl ?? ROUTES.MY_MARKS);

    if (error.message.length > 0)
      this.messageService.showGeneralError(error.message);

    return of(null);
  }


  //* Marks
  private fetchMark( url: string ): Observable<Mark> {
    // check if the url is to see a mark
    if (url.includes(this.SEE_MARK_ROUTE)) {
      const markId = this.getIdFromUrlParam();

      if (markId === undefined) {
        return throwError(() => new Error("Invalid mark ID"));
      }

      // Check if the mark is in local storage to return it.
      const localMark = this.markService.getMarkFromLocalStorage(markId);
      if (localMark) return of(localMark);
      
      return this.getMarkById(markId);
    }
    // Or to add a new mark
    if (url.includes(ROUTES.MARKS_NEW)) {
      return this.createEmptyMark();
    }
    // The url is not valid
    return throwError(() => new Error("Invalid route or action."));
  }

  private getMarkById( id: number ): Observable<Mark> {
    return this.markService.getById(id)
    .pipe(delay(500));
  }

  private createEmptyMark(): Observable<Mark> {
    const emptyMark : Mark = {
      id : 0,
      name: DEFAULT_MARK_NAME,
      collectionId: 0,
      creatorId : 0,
      blocks: [
        {
          id : 0,
          title : DEFAULT_BLOCK_NAME,
          content: '',
        }
      ]
    };

    return this.markService.create(emptyMark);
  }

  private updateMarkSyncStatus(state: boolean): void {
    this.currentMark.requiresSync = state;
  }

  private updateMarkWithRetry(mark: Mark): void {
    const { requiresSync } = mark;
    this.setSaveState(SaveState.saving);

    this.markService.update(mark)
    .pipe(
      // Error retry with exponential backoff
      retry(this.getErrorRetryConfig()),
      delay(500),
    )
    .subscribe({
      next:  (mark)  => {
        mark.requiresSync = requiresSync;
        this.form.reset(mark);
        this.setSaveState(SaveState.saved);

        if (this.currentMark.requiresSync) {
          this.updateMarkSyncStatus(false);
          this.markService.dropMarkFromLocalStorage(mark.id);
        }

      },
      // After maximum retries, set error state and buffer unsaved data.
      error: ()  => this.saveChangesLocally()
    });
  }

  private handleMarkNameInputDebounce(): Subscription {
    const DEBOUNCE_TIME_IN_MILLI_SECONDS = 1000;
    return this.markNameInputDebouncer
    .pipe(debounceTime(DEBOUNCE_TIME_IN_MILLI_SECONDS))
    .subscribe((name) => {
      let mark = {
        ...this.currentMark,
        name,
      } as Mark;
      this.updateMarkWithRetry(mark);
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

    this.currentBlocks.clear();
    blockControls.forEach(c => this.currentBlocks.push(c));
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

  public castAbstractControlToFormGroup(control: AbstractControl) {
    return control as FormGroup;
  }

  private getIdFromUrlParam(): number | undefined {
    const markId = this.activatedRoute.snapshot.paramMap.get(this.MARKID_PARAM_NAME);
    
    if (markId == null || isNaN(Number(markId))) {
      this.redirectToUrl(this.previousUrl ?? ROUTES.MY_MARKS);
      return undefined;
    }
    
    return Number(markId);
  }

  private changeFloatingMenuState( menuOptions ?: FloatingMenuOption[] ): void {
    if (menuOptions) this.floatingMenuOptions = menuOptions;
    this.isFloatingMenuVisible = !this.isFloatingMenuVisible;
  }

  private getErrorRetryConfig(): RetryConfig {
    const { maxEntries, delayInMs } = this.ERROR_RETRY_SETTINGS;
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
    this.markService.setMarkInLocalStorage(this.currentMark);
  }
}