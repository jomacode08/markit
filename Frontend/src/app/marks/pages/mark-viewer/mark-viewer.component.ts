import { AbstractControl, FormArray, FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AfterViewInit, Component, ViewChild, signal, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { catchError, delay, Observable, of, tap, throwError } from 'rxjs';

import { ButtonModule } from 'primeng/button';
import { Carousel, CarouselModule, CarouselPageEvent } from 'primeng/carousel';
import { ChipModule } from 'primeng/chip';
import { Editor } from '@tiptap/core';
import { SkeletonModule } from 'primeng/skeleton';
import { TooltipModule } from 'primeng/tooltip';

import { CustomMessageService } from '../../../shared/services/custom-message.service';
import { DEFAULT_MARK_NAME, ROUTES } from './../../../shared/interfaces/constant';
import { FloatingMenuComponent } from '../../../shared/components/layout/floating-menu/floating-menu.component';
import { FloatingMenuOption } from '../../../shared/components/layout/floating-menu/floating-menu-option';
import { Mark } from '../../interfaces/mark';
import { MarkService } from '../../services/mark.service';
import { ValidatorErrorField } from '../../../shared/utils/validator-error-field';
import { Block, BlockColors } from '../../interfaces/block';
import { BlockComponent } from '../../components/block/block.component';
import { CurrentRouteService } from '../../../shared/services/current-route.service';
import { ErrorFieldComponent } from '../../../shared/components/layout/error-field/error-field.component';
import { GeneralButtonComponent } from '../../../shared/components/ui/buttons/general-button.component';
import { DialogService, DynamicDialogRef } from 'primeng/dynamicdialog';
import { BlockMenuComponent } from '../../components/block-menu/block-menu.component';
import { DEFAULT_BLOCK_NAME } from '../../../shared/interfaces/constant';
import { createTextFormattingOptions } from '../../interfaces/text-formatting-options';

@Component({
  standalone: true,
  imports: [
    BlockComponent,
    ButtonModule,
    CarouselModule,
    CommonModule,
    ChipModule,
    ErrorFieldComponent,
    FloatingMenuComponent,
    GeneralButtonComponent,
    ReactiveFormsModule,
    SkeletonModule,
    TooltipModule
  ],
  providers: [DialogService],
  templateUrl: './mark-viewer.component.html',
  styleUrl: './mark-viewer.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MarkViewerComponent extends ValidatorErrorField implements OnInit, AfterViewInit {
  //* Configuration
  private editor = signal<Editor | undefined>(undefined);
  private blockMenuDialogRef: DynamicDialogRef | undefined;
  // Constants
  private readonly MARKID_PARAM_NAME : string = 'id';
  private readonly SEE_MARK_ROUTE : string = 'marks/see';
  // Routing
  private previousUrl : string | null = null;
  private currentUrl : string | null = null;
  // Floating menu
  public floatingMenuOptions : FloatingMenuOption[] = [];
  public isFloatingMenuVisible : boolean = false;
  public textFormattingOptions: FloatingMenuOption[] = [];
  // Block carousel  
  @ViewChild('blockCarousel') private carousel !: Carousel;
  public currentBlockIndex = signal<number>(0);

  //* Form
  public markLoader$ = new Observable<Mark | null>();
  public submit   : boolean = false;
  public form = new FormGroup({
    id       : new FormControl<number>(0),
    name     : new FormControl<string>("My new mark 🎉", [Validators.required, Validators.maxLength(255)]),
    blocks   : new FormArray<FormGroup>([]),
  });

  //* Getters
  get currentMark(): Mark {
    return this.form.value as Mark;
  }
  get currentBlocks() {
    return this.form.get('blocks') as FormArray;
  }

  //* Lyfecycle
  constructor(
    private router: Router,
    private activatedRoute: ActivatedRoute,
    private markService: MarkService,
    private messageService: CustomMessageService,
    private dialogService: DialogService,
    private fb: FormBuilder,
    private currentRouteService: CurrentRouteService
  ) {
    super();
    this.previousUrl = this.currentRouteService.previousSuccessfulUrl();
    this.currentUrl = this.currentRouteService.url();
    this.textFormattingOptions = createTextFormattingOptions(this.editor);
  }

  public ngOnInit(): void {
    this.markLoader$ = this.fetchMark(this.currentUrl ?? '').pipe(
      tap((mark) => this.initializeForm(mark)),
      catchError((error) => this.handleError(error))
    );
  }

  public ngAfterViewInit(): void {
    // set focus on the name input
    const inputElement: HTMLElement | null = document.getElementById('name');
    if (inputElement) inputElement.focus();
  }

  //* Events
  public onEditorSelected = (editor : Editor) => this.editor.set(editor);

  public onSubmit(): void {
    if (this.form.invalid) return this.form.markAllAsTouched();
    this.setSubmit(true);

    this.updateMark(this.currentMark);
  }

  public onCancel(): void {
    this.redirectToUrl(this.previousUrl ?? ROUTES.COLLECTION_EXPLORER);
  }

  public onTextFormattingButtonClick( ): void {
    this.changeFloatingMenuState(this.textFormattingOptions);
  }

  public onCarouselScroll( event: CarouselPageEvent ): void {
    if (event.page != null) this.currentBlockIndex.set(event.page);
  }

  public onCarouselIndicatorBtnClick(index: number): void {
    index > this.currentBlockIndex()
      ? this.carousel.navForward(new MouseEvent('click'), index)
      : this.carousel.navBackward(new MouseEvent('click'), index);  
    this.currentBlockIndex.set(index);
  }

  public openBlockMenuDialog( blocks : Block[] ): void {
    this.blockMenuDialogRef = this.dialogService.open(BlockMenuComponent, {
      header: 'Blocks',
      width : '30rem',
      modal : true,
      closable : false,
      dismissableMask : true,
      styleClass : 'custom-dialog',
      data: {
        blocks : structuredClone(blocks)
      }
    });

    this.blockMenuDialogRef.onClose.subscribe(( blocks ?: Block[] ) => {
      this.blockMenuDialogRef = undefined;
      if (blocks) {
        this.setBlocks(blocks);
      }
    });
  }

  //* Form
  private initializeForm(mark: Mark): void {
    this.setBlocks(mark.blocks);
    this.form.reset(mark);
  }

  private handleError(error: Error): Observable<null> {
    this.redirectToUrl(this.previousUrl ?? ROUTES.COLLECTION_EXPLORER);
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
    .pipe(delay(1000));
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
          cols: 12,
          color: BlockColors.transparent,
          content: ''
        }
      ]
    };

    return this.markService.create(emptyMark);
  }

  private updateMark(mark: Mark): void {
    this.markService.patch(mark).subscribe({
      next:  (mark)  => {
        this.form.reset(mark);
        this.setSubmit(false);
        this.messageService.showGeneralSuccess("Mark updated successfully");
      },
      error: ()  => this.setSubmit(false)
    });
  }

  //* Blocks
  private setBlocks(blocks: Block[]): void {
    const blockControls = blocks.map(block =>
      this.fb.group({
        id: [block.id, Validators.required],
        content: [block.content, Validators.required],
        title: [block.title, Validators.required],
        cols: [block.cols],
        color: [block.color]
      })
    );

    this.currentBlocks.clear();
    blockControls.forEach(c => this.currentBlocks.push(c));
  }

  //* UTILS
  public castAbstractControlToFormGroup(control: AbstractControl) {
    return control as FormGroup;
  }
  private getIdFromUrlParam(): number | undefined {
    const markId = this.activatedRoute.snapshot.paramMap.get(this.MARKID_PARAM_NAME);
    
    if (markId == null || isNaN(Number(markId))) {
      this.redirectToUrl(this.previousUrl ?? ROUTES.COLLECTION_EXPLORER);
      return undefined;
    }

    return Number(markId);
  }
  private setSubmit(value: boolean): void {
    this.submit = value;
  }
  private changeFloatingMenuState( menuOptions ?: FloatingMenuOption[] ): void {
    if (menuOptions) this.floatingMenuOptions = menuOptions;
    this.isFloatingMenuVisible = !this.isFloatingMenuVisible;
  }
  private redirectToUrl(url: string): void {
    this.router.navigate([url]);
  }
}