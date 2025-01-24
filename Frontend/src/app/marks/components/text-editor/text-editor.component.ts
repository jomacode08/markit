import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit, ViewEncapsulation, computed, forwardRef } from '@angular/core';
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
import { EditionOption } from '../../interfaces/edition-option';

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
export class TextEditorComponent implements OnDestroy, OnInit, ControlValueAccessor {
  //* Configuration
  public input: string = "";
  public isEditionMenuVisible: boolean = false;
  public currentEditionOptions: EditionOption[] = [];
  public mainEditionOptions = computed(() => [
    {
      label: 'Bold',
      icon: 'fa fa-bold',
      name: 'bold',
      command(editor: Editor) {
        editor.chain().focus().toggleBold().run();
      },
    },
    {
      label: 'Italic',
      name: 'italic',
      icon: 'fa fa-italic',
      command(editor: Editor) {
        editor.chain().focus().toggleItalic().run();
      },
    },
    {
      label: 'Underline',
      name: 'underline',
      icon: 'fa fa-underline',
      command(editor: Editor) {
        editor.chain().focus().toggleUnderline().run();
      },
    },
    {
      label: 'Align',
      icon: 'fa fa-align-left',
      name: 'alignment',
      children: [
        {
          label: 'Left',
          icon: 'fa fa-align-left',
          name: 'alignment',
          textAlign: 'left',
          command(editor: Editor) {
            editor.chain().focus().setTextAlign('left').run();
          },
        },
        {
          label: 'Center',
          icon: 'fa fa-align-center',
          name: 'alignment',
          textAlign: 'center',
          command(editor: Editor) {
            editor.chain().focus().setTextAlign('center').run();
          },
        },
        {
          label: 'Right',
          icon: 'fa fa-align-right',
          name: 'alignment',
          textAlign: 'right',
          command(editor: Editor) {
            editor.chain().focus().setTextAlign('right').run();
          },
        },
        {
          label: 'Justify',
          icon: 'fa fa-align-justify',
          name: 'alignment',
          textAlign: 'justify',
          command(editor: Editor) {
            editor.chain().focus().setTextAlign('justify').run();
          },
        },
      ],
    },
    {
      label: 'Heading',
      icon: 'fa fa-heading',
      name: 'heading',
      children: [
        {
          label: 'Heading 1',
          name: 'heading',
          icon: 'fa fa-heading',
          level: 1,
          command(editor: Editor) {
            editor.chain().focus().toggleHeading({ level: 1 }).run();
          },
        },
        {
          label: 'Heading 2',
          name: 'heading',
          icon: 'fa fa-heading',
          level: 2,
          command(editor: Editor) {
            editor.chain().focus().toggleHeading({ level: 2 }).run();
          },
        },
        {
          label: 'Heading 3',
          name: 'heading',
          icon: 'fa fa-heading',
          level: 3,
          command(editor: Editor) {
            editor.chain().focus().toggleHeading({ level: 3 }).run();
          },
        },
      ],
    },
    {
      label: 'Highlight',
      name: 'highlight',
      icon: 'fa fa-highlighter',
      children: [
        {
          label: 'Yellow',
          name: 'highlight',
          icon: 'fa fa-circle',
          color: '#f5f378',
          command(editor: Editor) {
            editor.chain().focus().toggleHighlight({ color: '#f5f378' }).run();
          }
        },
        {
          label: 'Purple',
          name: 'highlight',
          icon: 'fa fa-circle',
          color: '#dcc1ff',
          command(editor: Editor) {
            editor.chain().focus().toggleHighlight({ color: '#dcc1ff' }).run();
          }
        },
        {
          label: 'Orange',
          name: 'highlight',
          icon: 'fa fa-circle',
          color: '#ec704b',
          command(editor: Editor) {
            editor.chain().focus().toggleHighlight({ color: '#ec704b' }).run();
          }
        },
      ]
    },
    {
      label: 'List',
      icon: 'fa fa-list',
      name: 'bulletList',
      command(editor: Editor) {
        editor.chain().focus().toggleBulletList().run();
      },
    },
    {
      label: 'Code',
      icon: 'fa fa-code',
      name: 'codeBlock',
      command(editor: Editor) {
        editor.chain().focus().toggleCodeBlock().run();
      },
    },
    {
      label: 'Task',
      icon: 'fa fa-square-check',
      name: 'taskList',
      command(editor: Editor) {
        editor.chain().focus().toggleTaskList().run();
      },
    },
  ]);

  public editor = new Editor({
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
  ngOnDestroy(): void {
    this.editor.destroy();
  }

  ngOnInit(): void {
    this.currentEditionOptions = this.mainEditionOptions();
  }

  //* Methods
  setEditionMenuState(): void {
    this.isEditionMenuVisible = !this.isEditionMenuVisible;
  }

  isNodeMarkActive(EditionOption: EditionOption): boolean {
    const { name, level, textAlign, color } = EditionOption;

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

  getColorStyleDeclaration(color?: string): string {
    if (!color) return '';
    return `color: ${color};`;
  }
}
