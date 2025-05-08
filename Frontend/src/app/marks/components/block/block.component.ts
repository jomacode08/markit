import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output, ViewEncapsulation, forwardRef } from '@angular/core';
import { ControlValueAccessor, FormsModule, NG_VALUE_ACCESSOR } from '@angular/forms';

//* Tiptap Extensions
import { Editor } from '@tiptap/core';
import { NgxTiptapModule } from 'ngx-tiptap';
import CodeBlockLowlight from '@tiptap/extension-code-block-lowlight';
import Highlighter from '@tiptap/extension-highlight';
import Placeholder from '@tiptap/extension-placeholder';
import StarterKit from '@tiptap/starter-kit';
import TaskList from '@tiptap/extension-task-list';
import TaskItem from '@tiptap/extension-task-item';
import TextAlign from '@tiptap/extension-text-align';
import Underline from '@tiptap/extension-underline';

//* Lowlight Code block dependency
import { common, createLowlight } from 'lowlight'

import { SkeletonModule } from 'primeng/skeleton';

@Component({
  selector: 'marks-block',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    NgxTiptapModule,
    SkeletonModule,
  ],
  templateUrl: './block.component.html',
  styleUrl: './block.component.css',
  encapsulation: ViewEncapsulation.None,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => BlockComponent),
      multi: true
    }
  ]
})
export class BlockComponent implements ControlValueAccessor {
  //* Configuration
  @Input() public title: string = "";
  @Output() public onEditorSelected = new EventEmitter<Editor>();

  public input: string = "";
  public editor = new Editor({
    extensions: [
      StarterKit.configure({
        codeBlock: false,
      }),
      CodeBlockLowlight.configure({
        lowlight : createLowlight(common),
      }),
      Highlighter.configure({
        multicolor: true,
      }),
      TextAlign.configure({
        types: ['heading', 'paragraph'],
      }),      
      Placeholder.configure({
        placeholder: 'Type something here'
      }),
      TaskList,
      TaskItem.configure({
        nested: true,
      }),
      Underline
    ]
  });

  //* ControlValueAccessor implementation
  onChange: (value: any) => void = () => {};
  onTouched: () => void = () => {};

  writeValue(value: any): void {
    this.input = value;
  }

  registerOnChange(fn: any): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: any): void {
    this.onTouched = fn;
  }

  ngOnDestroy(): void {
    this.editor.destroy();
  }

  //* Methods
  onEditorClick = (): void => this.onEditorSelected.emit(this.editor);
}