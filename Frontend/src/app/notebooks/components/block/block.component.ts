import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Injector, OnInit, Output, ViewEncapsulation, forwardRef, inject } from '@angular/core';
import { ControlValueAccessor, FormsModule, NG_VALUE_ACCESSOR } from '@angular/forms';

//* Tiptap Extensions
import { Editor } from '@tiptap/core';
import { Markdown } from '@tiptap/markdown';
import { AngularNodeViewRenderer, TiptapEditorDirective } from 'ngx-tiptap';
import CodeBlockLowlight from '@tiptap/extension-code-block-lowlight';
import Highlighter from '@tiptap/extension-highlight';
import Link from '@tiptap/extension-link'
import Placeholder from '@tiptap/extension-placeholder';
import StarterKit from '@tiptap/starter-kit';
import { Table, TableCell, TableHeader, TableRow } from '@tiptap/extension-table'
import TaskItem from '@tiptap/extension-task-item';
import TaskList from '@tiptap/extension-task-list';
import TextAlign from '@tiptap/extension-text-align';
import Underline from '@tiptap/extension-underline';

//* Lowlight Code block dependency
import { common, createLowlight } from 'lowlight'

import { SkeletonModule } from 'primeng/skeleton';
import { debounceTime, Subject } from 'rxjs';
import GistBlockExtension from '../../extensions/gist-block.extension';
import { isWebUrl } from '../../utils/link-url';
import { MarkdownLinkInputRule } from '../../extensions/markdown-link-input-rule.extension';
import { TableNodeViewComponent } from '../tiptap/table-node-view/table-node-view.component';
import { deleteTableSelection } from '../../../shared/utils/tiptap';

@Component({
    selector: 'notebooks-block',
    imports: [
      CommonModule,
      FormsModule,
      SkeletonModule,
      TiptapEditorDirective,
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
export class BlockComponent implements OnInit, ControlValueAccessor {
  //* Configuration
  @Output() public editorSelected = new EventEmitter<Editor>();
  @Output() public debouncedChange = new EventEmitter<string>();
  private injector = inject(Injector);
  
  private debouncer = new Subject<string>();
  public editor = new Editor({
    extensions: [
      CodeBlockLowlight.configure({
        lowlight : createLowlight(common),
      }),
      GistBlockExtension(this.injector),
      Highlighter.configure({
        multicolor: true,
      }),
      Link.configure({
        openOnClick: false,
        autolink: true,
        linkOnPaste: true,
        defaultProtocol: 'https',
        isAllowedUri: (url: string) => isWebUrl(url),
      }),
      Markdown,
      MarkdownLinkInputRule,
      Placeholder.configure({
        placeholder: 'Type something here'
      }),
      StarterKit.configure({
        codeBlock: false,
      }),
      Table
      .extend({
        addNodeView() {
          return AngularNodeViewRenderer(
            TableNodeViewComponent,
            {
              injector: inject(Injector),
            }
          )
        },
        addKeyboardShortcuts() {
          return {
            ...this.parent?.(),
            Backspace: () => deleteTableSelection(this.editor),
          }
        }
      }),
      TableCell,
      TableRow,
      TableHeader,
      TextAlign.configure({
        types: ['heading', 'paragraph'],
      }),
      TaskList,
      TaskItem.configure({
        nested: true,
      }),
      Underline
    ],
    contentType: 'markdown',
    onUpdate: ({editor}) => {
      const content = editor.getMarkdown();
      this.onChange(content);
      this.debouncer.next(content);
    }
  });

  //* Lyfecycle hooks
  ngOnInit(): void {
    this.editor.commands.focus('end');
    this.editorSelected.emit(this.editor);
    this.debouncer.pipe(
      debounceTime(1000)
    ).subscribe((value) => this.debouncedChange.emit(value));
  }

  ngOnDestroy(): void {
    this.editor.destroy();
    this.debouncer.unsubscribe();
  }

  //* ControlValueAccessor implementation
  onChange: (value: string) => void = () => {};
  onTouched: () => void = () => {};

  writeValue(value: string): void {
    this.editor.commands.setContent(value ?? '', {
      contentType: 'markdown',
      emitUpdate: false
    });
  }

  registerOnChange(fn: any): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: any): void {
    this.onTouched = fn;
  }
}