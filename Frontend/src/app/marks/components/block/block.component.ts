import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Injector, Input, OnInit, Output, ViewEncapsulation, forwardRef, inject } from '@angular/core';
import { ControlValueAccessor, FormsModule, NG_VALUE_ACCESSOR } from '@angular/forms';
import { LinkProtocolOptions } from './../../../../../node_modules/@tiptap/extension-link/dist/link.d';

//* Tiptap Extensions
import { Editor } from '@tiptap/core';
import { NgxTiptapModule } from 'ngx-tiptap';
import CodeBlockLowlight from '@tiptap/extension-code-block-lowlight';
import Highlighter from '@tiptap/extension-highlight';
import Link from '@tiptap/extension-link'
import Placeholder from '@tiptap/extension-placeholder';
import StarterKit from '@tiptap/starter-kit';
import TaskList from '@tiptap/extension-task-list';
import TaskItem from '@tiptap/extension-task-item';
import TextAlign from '@tiptap/extension-text-align';
import Underline from '@tiptap/extension-underline';

//* Lowlight Code block dependency
import { common, createLowlight } from 'lowlight'

import { SkeletonModule } from 'primeng/skeleton';
import { debounceTime, Subject } from 'rxjs';
import GistBlockExtension from '../../extensions/gist-block.extension';

type UriValidationContext = {
  defaultValidate: (url: string) => boolean;
  protocols: Array<LinkProtocolOptions | string>;
  defaultProtocol: string;
}

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
export class BlockComponent implements OnInit, ControlValueAccessor {
  //* Configuration
  @Input() public title: string = "";
  @Output() public onEditorSelected = new EventEmitter<Editor>();
  @Output() public onValueChange = new EventEmitter<string>();
  private injector = inject(Injector);
  
  private debouncer = new Subject<string>();
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
      Link.configure({
        openOnClick: true,
        autolink: true,
        defaultProtocol: 'https',
        protocols: ['http', 'https'],
        isAllowedUri: (url, ctx) => this.isValidUri(url, ctx)
      }).extend({
        inclusive: false,
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
      Underline,
      GistBlockExtension(this.injector)
    ]
  });

  //* Lyfecycle hooks
  ngOnInit(): void {
    this.debouncer.pipe(
      debounceTime(1000)
    ).subscribe((value) => this.onValueChange.emit(value));
  }

  ngOnDestroy(): void {
    this.editor.destroy();
    this.debouncer.unsubscribe();
  }

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

  //* Events
  onEditorClick = (): void => this.onEditorSelected.emit(this.editor);
  onEditorInputChange(value: string): void {
    this.input = value;
    this.onChange(value);
    this.debouncer.next(value);
  }
  //* Utils
  isValidUri(url: string, ctx: UriValidationContext): boolean {
    try {
      // construct URL
      const parsedUrl = url.includes(':') ? new URL(url) : new URL(`${ctx.defaultProtocol}://${url}`)

      // use default validation
      if (!ctx.defaultValidate(parsedUrl.href)) {
        return false
      }

      // disallowed protocols
      const disallowedProtocols = ['ftp', 'file', 'mailto']
      const protocol = parsedUrl.protocol.replace(':', '')

      if (disallowedProtocols.includes(protocol)) {
        return false
      }

      // only allow protocols specified in ctx.protocols
      const allowedProtocols = ctx.protocols.map(p => (typeof p === 'string' ? p : p.scheme))

      if (!allowedProtocols.includes(protocol)) {
        return false
      }

      // all checks have passed
      return true
    } catch {
      return false
    }
  }
}