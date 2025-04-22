import { AbstractControl, FormArray, FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AfterViewInit, Component, inject, OnInit, ViewEncapsulation } from '@angular/core';
import { delay } from 'rxjs';

import { Editor } from '@tiptap/core';
import { ButtonModule } from 'primeng/button';
import { SkeletonModule } from 'primeng/skeleton';

import { CustomMessageService } from '../../../shared/services/custom-message.service';
import { FloatingMenuComponent } from '../../../shared/components/layout/floating-menu/floating-menu.component';
import { FloatingMenuOption } from '../../../shared/components/layout/floating-menu/floating-menu-option';
import { Cols, Mark } from '../../interfaces/mark';
import { MarkService } from '../../services/mark.service';
import { NodeMark } from '../../interfaces/node-mark';
import { ValidatorErrorField } from '../../../shared/utils/validator-error-field';
import { BlockColors } from '../../interfaces/block';
import { BlockComponent } from '../../components/block/block.component';
import { CurrentRouteService } from '../../../shared/services/current-route.service';
import { ErrorFieldComponent } from '../../../shared/components/layout/error-field/error-field.component';
import { GeneralButtonComponent } from '../../../shared/components/ui/buttons/general-button.component';

@Component({
  standalone: true,
  imports: [
    BlockComponent,
    ButtonModule,
    CommonModule,
    ErrorFieldComponent,
    FloatingMenuComponent,
    GeneralButtonComponent,
    ReactiveFormsModule,
    SkeletonModule,
  ],
  templateUrl: './mark-viewer.component.html',
  styleUrl: './mark-viewer.component.css',
  encapsulation: ViewEncapsulation.None
})
export class MarkViewerComponent extends ValidatorErrorField implements OnInit, AfterViewInit {
  //* Services
  private markService     : MarkService = inject(MarkService);
  private messageService  : CustomMessageService = inject(CustomMessageService);

  //* Configuration
  private previousUrl : string | null = null;
  public floatingMenuOptions : FloatingMenuOption[] = []
  public isFloatingMenuVisible : boolean = false;

  //* Form
  public submit   : boolean = false;
  public loading ?: boolean;
  public form = new FormGroup({
    id       : new FormControl<number>(0),
    name     : new FormControl<string>("My new mark 🎉", [Validators.required, Validators.maxLength(255)]),
    blocks   : new FormArray<FormGroup<any>>([]),
  });

  //* Mark Menu
  public editor ?: Editor;
  public fontOptions : FloatingMenuOption[] = [
    {
      label: 'Bold',
      icon: 'fa fa-bold',
      command: () => this.editor?.chain().focus().toggleBold().run(),
      isActive: () => this.isNodeMarkActive({ name: 'bold' }),
    },
    {
      label: 'Italic',
      icon: 'fa fa-italic',
      command: () => this.editor?.chain().focus().toggleItalic().run(),
      isActive: () => this.isNodeMarkActive({ name: 'italic' }),
    },
    {
      label: 'Underline',
      icon: 'fa fa-underline',
      command: () => this.editor?.chain().focus().toggleUnderline().run(),
      isActive: () => this.isNodeMarkActive({ name: 'underline' }),
    },
    {
      label: 'Align',
      icon: 'fa fa-align-left',
      children: [
        {
          label: 'Left',
          icon: 'fa fa-align-left',
          command: () => this.editor?.chain().focus().setTextAlign('left').run(),
          isActive: () => {
            return this.isNodeMarkActive({
              name: 'alignment',
              textAlign: 'left'
            });
          },
        },
        {
          label: 'Center',
          icon: 'fa fa-align-center',
          command: () => this.editor?.chain().focus().setTextAlign('center').run(),
          isActive: () => {
            return this.isNodeMarkActive({
              name: 'alignment',
              textAlign: 'center'
            });
          },
        },
        {
          label: 'Right',
          icon: 'fa fa-align-right',
          command: () => this.editor?.chain().focus().setTextAlign('right').run(),
          isActive: () => {
            return this.isNodeMarkActive({
              name: 'alignment',
              textAlign: 'right'
            });
          },
        },
        {
          label: 'Justify',
          icon: 'fa fa-align-justify',
          command: () => this.editor?.chain().focus().setTextAlign('justify').run(),
          isActive: () => {
            return this.isNodeMarkActive({
              name: 'alignment',
              textAlign: 'justify'
            });
          },
        },
      ],
    },
    {
      label: 'Heading',
      icon: 'fa fa-heading',
      children: [
        {
          label: 'Heading 1',
          icon: 'fa fa-heading',
          command: () => this.editor?.chain().focus().toggleHeading({ level: 1 }).run(),
          isActive: () => {
            return this.isNodeMarkActive({
              name: 'heading',
              level: 1
            });
          },
        },
        {
          label: 'Heading 2',
          icon: 'fa fa-heading',
          command: () => this.editor?.chain().focus().toggleHeading({ level: 2 }).run(),
          isActive: () => {
            return this.isNodeMarkActive({
              name: 'heading',
              level: 2
            });
          },
        },
        {
          label: 'Heading 3',
          icon: 'fa fa-heading',
          command: () => this.editor?.chain().focus().toggleHeading({ level: 3 }).run(),
          isActive: () => {
            return this.isNodeMarkActive({
              name: 'heading',
              level: 3
            });
          },
        },
      ],
    },
    {
      label: 'Highlight',
      icon: 'fa fa-highlighter',
      children: [
        {
          label: 'Yellow',
          icon: 'fa fa-circle',
          color: "#f5f378",
          command: () => this.editor?.chain().focus().toggleHighlight({ color: '#f5f378' }).run(),
          isActive: () => {
            return this.isNodeMarkActive({
              name: 'highlight',
              color: "#f5f378"
            })
          },
        },
        {
          label: 'Purple',
          icon: 'fa fa-circle',
          color: "#dcc1ff",
          command: () => this.editor?.chain().focus().toggleHighlight({ color: '#dcc1ff' }).run(),
          isActive: () => {
            return this.isNodeMarkActive({
              name: 'highlight',
              color: "#dcc1ff"
            })
          },
        },
        {
          label: 'Orange',
          icon: 'fa fa-circle',
          color: "#ec704b",
          command: () => this.editor?.chain().focus().toggleHighlight({ color: '#ec704b' }).run(),
          isActive: () => {
            return this.isNodeMarkActive({
              name: 'highlight',
              color: "#ec704b"
            })
          },
        },
      ]
    },
    {
      label: 'List',
      icon: 'fa fa-list',
      command: () => this.editor?.chain().focus().toggleBulletList().run(),
      isActive: () => this.isNodeMarkActive({ name: 'bulletList' }),
    },
    {
      label: 'Code',
      icon: 'fa fa-code',
      command: () => this.editor?.chain().focus().toggleCodeBlock().run(),
      isActive: () => this.isNodeMarkActive({ name: 'codeBlock' }),
    },
    {
      label: 'Task',
      icon: 'fa fa-square-check',
      command: () => this.editor?.chain().focus().toggleTaskList().run(),
      isActive: () => this.isNodeMarkActive({ name: 'taskList' }),
    },
  ];

  //* Block Menu
  public currentBlockIndex  : number = 0;
  public blockOptions : FloatingMenuOption[] = [
    {
      label: 'Add block',
      icon: 'fa fa-plus',
      command: () => {
        this.addBlock(this.currentBlockIndex + 1, 12);
        this.changeFloatingMenuState();
      }
    },
    {
      label: 'Remove',
      icon: 'fa fa-trash',
      command: () => {
        this.removeBlock(this.currentBlockIndex);
        this.changeFloatingMenuState();
      }
    },
    {
      label: 'Color',
      icon: 'fa fa-palette',
      children: [
        {
          label: 'Neutral',
          icon: 'fa fa-circle',
          color: '#fbf4e9',
          command: () => this.changeBlockColor(this.currentBlockIndex, BlockColors.neutral)
        },
        {
          label: 'Purple',
          icon: 'fa fa-circle',
          color: '#E8DCF9',
          command: () => this.changeBlockColor(this.currentBlockIndex, BlockColors.purple)
        },
        {
          label: 'Red',
          icon: 'fa fa-circle',
          color: '#EDCAC0',
          command: () => this.changeBlockColor(this.currentBlockIndex, BlockColors.red)
        },
      ]
    },
  ];

  //* Getters
  get currentMark(): Mark {
    return this.form.value as Mark;
  }

  get currentBlocks() {
    return this.form.controls["blocks"] as FormArray;
  }

  //* Lyfecycle
  constructor(
    private fb: FormBuilder,
    private router: Router,
    private activatedRoute: ActivatedRoute,
    private currentRouteService: CurrentRouteService
  ) {
    super();
    this.previousUrl = this.currentRouteService.previousSuccessfulUrl();
  }

  public ngOnInit(): void {
    // check if the route is to see a mark
    if (this.router.url.includes('see')) {
      // set the loading flag to true
      this.loading = true;
      // get the mark id from the route
      const markId = this.activatedRoute.snapshot.paramMap.get('id');
      if (markId == null || isNaN(Number(markId)))
        return this.redirectToUrl(this.previousUrl ?? 'marks');
      // get the mark and set it in the form
      this.setMark(Number(markId));
      return;
    }

    // Otherwise, the action is 'add a new mark' so we need to add the default block 
    this.addBlock(0, 12);
  }

  public ngAfterViewInit(): void {
    // set focus on the name input
    const inputElement: HTMLElement | null = document.getElementById('name');
    if (inputElement) inputElement.focus();
  }

  //* Events
  public onEditorSelected = (editor : Editor) => this.editor = editor;

  public onSubmit(): void {
    if (this.form.invalid) return this.form.markAllAsTouched();
    this.setSubmit(true);

    if (this.currentMark.id === 0) {
      this.addMark(this.currentMark);
    } else {
      this.updateMark(this.currentMark);
    }
  }

  public onCancel(): void {
    this.redirectToUrl(this.previousUrl ?? 'marks');
  }

  public onBlockTitleChanged(index: number, title: string ): void {
    const block = this.currentBlocks.at(index) as FormGroup;
    block.controls['title'].setValue(title); 
  }

  public onBlockOptionsButtonClick( index: number ): void {
    this.currentBlockIndex = index;
    this.changeFloatingMenuState(this.blockOptions);
  }

  public onFontOptionsButtonClick( ): void {
    this.changeFloatingMenuState(this.fontOptions);
  }

  //* Marks
  private setMark(markId: number) {
    this.markService.getById(markId)
    .pipe(
      delay(1000)
    )
    .subscribe(
      {
        error: (error) => this.redirectToUrl(this.previousUrl ?? 'marks'),
        next : (mark)  => {
          // Construct the blocks
          const blockControls = mark.blocks.map(block => this.fb.group(block));
          blockControls.forEach(control => this.currentBlocks.push(control));

          // Reset the form
          this.form.reset(mark);
          this.loading = false;
        }
      }
    );
  }

  private addMark(mark: Mark): void {
    this.markService.create(mark).subscribe({
      next:  (mark)  => {
        this.form.reset(mark);
        this.setSubmit(false);
        this.messageService.showGeneralSuccess("Mark created successfully");
      },
      error: ()  => this.setSubmit(false)
    });
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

  public isNodeMarkActive(nodeMark : NodeMark): boolean {
    const { name, level, color, textAlign } = nodeMark;

    if (this.editor === undefined || name === undefined) return false;

    switch (name) {
      case 'heading':
        if (level === undefined) return false;
        return this.editor.isActive(name, { level });

      case 'alignment':
        if (textAlign === undefined) return false;
        return this.editor.isActive({ textAlign });

      case 'highlight':
        if (color === undefined) return false;
        return this.editor.isActive(name, { color });

      default:
        return this.editor.isActive(name);
    }
  }

  //* Blocks
  public  addBlock(index : number, cols: Cols): void {
    const color: BlockColors = index > 0
    ? BlockColors.neutral
    : BlockColors.transparent;

    const newBlock = this.fb.group({
      id      : 0,
      title   : "",
      content : "",
      cols    : [cols, Validators.required],
      color   : [color, Validators.required]
    });
    
    this.currentBlocks.insert(index ,newBlock);
  }

  private removeBlock(index : number): void {
    const block = this.currentBlocks.at(index) as FormGroup;
    const content = block.controls['content'].value as string;

    // The content of the block is empty so it can be deleted
    if (content.length === 0) return this.currentBlocks.removeAt(index);
    // Otherwise the block has any content, show a warning to the user
    this.messageService.showConfirmationDialog({
      message: "Do you want to delete this block?. You won't be able to get it back later. ",
      header: 'Delete block',
      icon: 'fa fa-warning',
      accept: () => this.currentBlocks.removeAt(index)
    });
  }

  private changeBlockColor(index : number, color: BlockColors): void {
    const block = this.currentBlocks.at(index) as FormGroup;
    block.controls['color'].setValue(color);
  }

  //* UTILS
  private setSubmit(value: boolean): void {
    this.submit = value;
  }
  public changeFloatingMenuState( menuOptions ?: FloatingMenuOption[] ): void {
    if (menuOptions) this.floatingMenuOptions = menuOptions;
    this.isFloatingMenuVisible = !this.isFloatingMenuVisible;
  }
  public castAbstractControlToFormGroup(control: AbstractControl) {
    return control as FormGroup;
  }
  private redirectToUrl(url: string): void {
    this.router.navigate([url]);
  }
}
