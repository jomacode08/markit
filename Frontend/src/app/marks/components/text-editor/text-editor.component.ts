import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnChanges, OnDestroy, OnInit, Output, SimpleChanges, ViewEncapsulation, forwardRef } from '@angular/core';
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
import { createLowlight } from 'lowlight'
import csharp from 'highlight.js/lib/languages/csharp'
import css from 'highlight.js/lib/languages/css'
import html from 'highlight.js/lib/languages/xml'
import js from 'highlight.js/lib/languages/javascript'
import ts from 'highlight.js/lib/languages/typescript'
const lowlight = createLowlight({ html, css, js, ts, csharp });

import { PrimengModule } from '../../../shared/primeng/primeng.module';
import { BackColors } from '../../interfaces/block';

@Component({
  selector: 'marks-text-editor',
  standalone: true,
  imports: [
    PrimengModule,
    CommonModule,
    NgxTiptapModule,
    FormsModule
  ],
  templateUrl: './text-editor.component.html',
  styleUrl: './text-editor.component.css',
  encapsulation: ViewEncapsulation.None,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => TextEditorComponent),
      multi: true
    }
  ]
})
export class TextEditorComponent implements OnChanges, OnDestroy, ControlValueAccessor {
  //* Configuration
  @Input() public color: BackColors = BackColors.neutral;
  @Output() public onEditorSelected = new EventEmitter<Editor>();
  @Output() public onOptionsButtonClicked = new EventEmitter<void>();

  public input: string = "";
  public editor = new Editor({
    editorProps: {
      attributes: {
        class : this.color
      }
    },
    extensions: [
      StarterKit.configure({
        codeBlock: false,
      }),
      CodeBlockLowlight.configure({
        lowlight,
      }),
      Highlighter.configure({
        multicolor: true,
      }),
      TextAlign.configure({
        types: ['heading', 'paragraph'],
      }),      
      Placeholder.configure({
        placeholder: 'Type something here...'
      }),
      TaskItem.configure({
        nested: true,
      }),
      TaskList,
      Underline,
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

  //* Lifecycle
  ngOnChanges(changes: SimpleChanges): void {
    if (changes['color'].currentValue === undefined) return;

    this.color = changes['color'].currentValue;
    this.editor.setOptions({
      editorProps: {
        attributes: {
          class : this.color
        }
      }
    });
    
  }

  ngOnDestroy(): void {
    this.editor.destroy();
  }

  //* Methods
  onEditorClick(): void {
    this.onEditorSelected.emit(this.editor)
  }
}
