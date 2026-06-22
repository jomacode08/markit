import { FormArray, FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { catchError, debounceTime, delay, Observable, of, retry, RetryConfig, Subject, Subscription, switchMap, take, takeUntil, tap, timer } from 'rxjs';
import { CommonModule } from '@angular/common';
import { Component, signal, OnInit, OnDestroy, inject, ChangeDetectionStrategy, ViewChild } from '@angular/core';

import { ButtonModule } from 'primeng/button';
import { DialogService, DynamicDialogRef } from 'primeng/dynamicdialog';
import { Editor, getMarkRange, Range } from '@tiptap/core';
import { MessageService } from 'primeng/api';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { SkeletonModule } from 'primeng/skeleton';
import { TooltipModule } from 'primeng/tooltip';

import { Block } from '../../interfaces/block';
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
import { LinkDialogCloseResponse, LinkDialogComponent } from '../../components/tiptap/link-dialog/link-dialog.component';
import { Notebook } from '../../interfaces/notebook';
import { NOTEBOOK_VIEWER_CONSTANTS } from './constants/notebook-viewer-constants';
import { NotebookAutosaveIndicatorComponent, SaveState } from '../../components/notebook-autosave-indicator/notebook-autosave-indicator.component';
import { NotebookBreadcrumbComponent } from '../../components/notebook-breadcrumb/notebook-breadcrumb.component';
import { NotebookService } from '../../services/notebook.service';
import { ROUTES } from '../../../shared/utils/constant';
import { SharedData } from '../../components/block-menu/block-menu.component';

@Component({
    imports: [
      BlockComponent,
      BlockNavigatorComponent,
      ButtonModule,
      CommonModule,
      EmojiPickerComponent,
      FloatingActionButtonComponent,
      FloatingMenuComponent,
      NotebookAutosaveIndicatorComponent,
      NotebookBreadcrumbComponent,
      ProgressSpinnerModule,
      ReactiveFormsModule,
      SkeletonModule,
      TooltipModule,
    ],
    providers: [DialogService],
    templateUrl: './notebook-viewer.component.html',
    styleUrl: './notebook-viewer.component.css',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class NotebookViewerComponent implements OnInit, OnDestroy, CanComponentDeactivate {
  //* Configuration
  @ViewChild('floatingMenu') private floatingMenu !: FloatingMenuComponent;
  private blockMenuDialogRef ?: DynamicDialogRef<BlockMenuComponent> | null;
  private linkDialogRef ?: DynamicDialogRef<LinkDialogComponent> | null;
  private destroy$ = new Subject<void>();
  private fb = inject(FormBuilder);

  //* Form
  public notebookForm = this.fb.nonNullable.group({
    id: [0],
    inputName: ['My new notebook 🎉', [Validators.required, Validators.maxLength(255)]],
    emoji: [undefined as string | undefined],
    blocks : this.fb.array<FormGroup>([]),
    // Non-editable properties
    name: [''],
    collectionId: [0],
    userId: [''],
    collectionName: [''],
    requiresSync: [false]
  });
  private editor = signal<Editor | undefined>(undefined)
  public saveState  = signal<SaveState>(SaveState.idle);

  //* State management
  protected notebook$ : Observable<Notebook | null>;
  private notebookNameInputDebouncer = new Subject<string>();
  public currentNotebook = signal<Notebook>(this.notebookForm.getRawValue() as Notebook);
  public currentBlock = signal<Block|undefined>(undefined);
  public currentBlockIndex = signal<number>(0);

  //* Floating menu
  public floatingMenuOptions = signal<FloatingMenuOption[]>([]);

  //* Getters
  get notebookNamePlaceHolder(): string {
    return NOTEBOOK_VIEWER_CONSTANTS.NOTEBOOK_NAME_PLACEHOLDER;
  }
  get notFoundPlaceHolder(): string {
    return NOTEBOOK_VIEWER_CONSTANTS.NOT_FOUND_PLACEHOLDER;
  }

  //* Lyfecycle
  constructor(
    private activatedRoute: ActivatedRoute,
    private blockService: BlockService,
    private currentRouteService: CurrentRouteService,
    private dialogService: DialogService,
    private notebookService: NotebookService,
    private messageService: CustomMessageService,
    private router: Router,
    private toastService: MessageService,
  ) {
    this.notebook$ = this.activatedRoute.params
    .pipe(
      // Get the notebookId param and fetch notebook data
      switchMap((params) => {
        const notebookId = this.getValidNotebookId(params.id);
        return this.fetchNotebook(notebookId);
      }),
      // Initialize form and check notebook sync.
      tap(notebook => {
        this.initializeForm(notebook);
        if (notebook.requiresSync) this.updateNotebookWithRetry();
      }),
      catchError(error => {
        this.redirectOnError();
        return of(null);
      })
    );
  }
  
  public async ngOnInit(): Promise<void> {
    this.floatingMenuOptions.set(createTextFormattingOptions(this.editor, {
      openLinkDialog: () => this.openLinkDialog(),
    }));
    this.subscribeToFormChanges();
    this.subscribeToEmojiChanges();
    this.subscribeToDebouncedNotebookNameInput();
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
    this.redirectToUrl(this.currentRouteService.previousSuccessfulUrl() ?? ROUTES.LIBRARY);
  }

  public onTextFormattingButtonClick(): void {
    this.changeFloatingMenuState();
  }

  public retryNotebookSave(): void {
    this.updateNotebookWithRetry();
  }

  public onNotebookNameInputChanged(event: Event) {
    let name = (event.target as HTMLInputElement).value;
    this.notebookNameInputDebouncer.next(name);
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

  public openLinkDialog(): void {
    const editor = this.editor();
    if (!editor) return;

    const range = this.getCurrentLinkRange(editor);
    const selectedText = editor.state.doc.textBetween(range.from, range.to, ' ');
    const linkAttributes = editor.getAttributes('link');

    this.linkDialogRef = this.dialogService.open(LinkDialogComponent, {
      header: 'Link',
      width : '30rem',
      modal : true,
      closable : true,
      dismissableMask : true,
      styleClass : 'custom-dialog',
      data: {
        shared : {
          text: selectedText,
          url: linkAttributes['href'] ?? '',
        }
      }
    });

    this.linkDialogRef?.onClose
    .pipe(take(1))
    .subscribe((response: LinkDialogCloseResponse | undefined) => {
      this.linkDialogRef = null;
      if (!response) return;
      this.applyLinkToEditor(editor, range, response);
    });
  }

  private async handleOnCloseBlockMenu(response: OnCloseResponse): Promise<void> {
    // Clean dialog reference
    this.blockMenuDialogRef = null;
    if (response == null) return;
    // A new block was selected
    if (response.selectedBlockId != null) {
      const currentNotebook = this.currentNotebook();
      const selectedBlockIndex = currentNotebook.blocks.findIndex(b => b.id === response.selectedBlockId);
      this.modifyActiveBlock(selectedBlockIndex);
      return;
    }
    // Apply block changes
    if (response.blocks) {
      await this.setBlocks(response.blocks);
      this.updateNotebookWithRetry();
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
    const blocks = this.notebookForm.get('blocks') as FormArray;
    return blocks.at(this.currentBlockIndex()) as FormGroup;
  }
  private initializeForm(notebook: Notebook): void {
    this.notebookForm.reset(notebook, {emitEvent: false});
    this.setBlocks(notebook.blocks);
    this.modifyActiveBlock(0);
  }

  private subscribeToFormChanges(): Subscription {
    return this.notebookForm.valueChanges
    .pipe(takeUntil(this.destroy$))
    .subscribe(changes => {
      this.currentNotebook.update(current => changes as Notebook);
    });
  }
  
  private subscribeToEmojiChanges(): Subscription {
    return this.notebookForm.controls.emoji.valueChanges
    .pipe(takeUntil(this.destroy$))
    .subscribe(emoji => {
      if (this.currentNotebook().emoji != emoji) {
        this.updateNotebookWithRetry();
      }
    });
  }

  private redirectOnError(): void {
    this.redirectToUrl(ROUTES.ERROR);
  }

  //* Notebooks
  private fetchNotebook(notebookId : number): Observable<Notebook> {
    const localNotebook = this.notebookService.getNotebookFromLocalStorage(notebookId);
    if (localNotebook) return of(localNotebook);
    return this.getNotebookById(notebookId);
  }

  private getNotebookById( id: number ): Observable<Notebook> {
    return this.notebookService.getById(id);
  }

  private updateNotebookSyncStatus(state: boolean): void {
    this.notebookForm.value.requiresSync = state;
  }

  private resetNotebookSync(notebookId: number): void {
    this.updateNotebookSyncStatus(false);
    this.notebookService.dropNotebookFromLocalStorage(notebookId);
  }

  private updateNotebookWithRetry(): void {
    this.setSaveState(SaveState.saving);
    const currentNotebook = this.notebookForm.getRawValue() as Notebook;
    this.notebookService.update(currentNotebook)
    .pipe(
      // Error retry with exponential backoff
      retry(this.getErrorRetryConfig()),
      delay(500),
    )
    .subscribe({
      next:  (notebook)  => {
        this.notebookForm.reset(notebook);
        this.setSaveState(SaveState.saved);
        if (currentNotebook.requiresSync)
          this.resetNotebookSync(currentNotebook.id);
      },
      // After maximum retries, set error state and buffer unsaved data.
      error: ()  => this.saveChangesLocally()
    });
  }

  private subscribeToDebouncedNotebookNameInput(): Subscription {
    const DEBOUNCE_TIME_IN_MILLI_SECONDS = 1000;
    return this.notebookNameInputDebouncer
    .pipe(
      takeUntil(this.destroy$),
      debounceTime(DEBOUNCE_TIME_IN_MILLI_SECONDS)
    )
    .subscribe((name) => {
      this.notebookForm.value.name = name;
      this.updateNotebookWithRetry();
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

    this.notebookForm.controls.blocks.clear();
    blockControls.forEach(c => this.notebookForm.controls.blocks.push(c));
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
    const currentNotebook = this.currentNotebook();
    if (newIndex < 0 || newIndex >= currentNotebook.blocks.length) return;
    this.currentBlockIndex.update(c => newIndex);
    this.currentBlock.update(c => currentNotebook.blocks[newIndex]);
    this.editor()?.commands.focus('end');
  }

  private getValidNotebookId(notebookIdParam: string): number {
    if (notebookIdParam == null || isNaN(Number(notebookIdParam))) {
      throw new Error("The notebookId is invalid");
    }
    return Number(notebookIdParam);
  }

  private changeFloatingMenuState(): void {
    this.floatingMenu.toggle();
  }

  private getCurrentLinkRange(editor: Editor): Range {
    const { selection, schema } = editor.state;
    const linkMark = schema.marks['link'];

    if (selection.empty && linkMark) {
      const linkRange = getMarkRange(selection.$from, linkMark);
      if (linkRange) return linkRange;
    }

    return {
      from: selection.from,
      to: selection.to,
    };
  }

  private applyLinkToEditor(
    editor: Editor,
    range: Range,
    response: LinkDialogCloseResponse
  ): void {
    const text: string = response.text.trim();
    const to: number = range.from + text.length;

    editor
      .chain()
      .focus()
      .insertContentAt(range, text)
      .setTextSelection({ from: range.from, to })
      .setLink({ href: response.href })
      .setTextSelection(to)
      .run();
  }

  private getErrorRetryConfig(): RetryConfig {
    const { maxEntries, delayInMs } = NOTEBOOK_VIEWER_CONSTANTS.ERROR_RETRY_SETTINGS;
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
    this.updateNotebookSyncStatus(true);
    this.notebookService.setNotebookInLocalStorage(this.currentNotebook());
  }
}
