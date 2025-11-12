import { ChangeDetectionStrategy, Component, computed, EventEmitter, input, OnInit, Output, signal } from '@angular/core';
import { DatePipe } from '@angular/common';

import { CodeEditor } from '@acrodata/code-editor';
import { DynamicDialogRef, DialogService, DynamicDialogConfig } from 'primeng/dynamicdialog';
import { languages } from '@codemirror/language-data';

import { Gist, GistFile } from '../../interfaces/gist';
import { GistFilePickerComponent, GistFilePickerSharedData } from '../gist-file-picker/gist-file-picker.component';
import { FormsModule } from '@angular/forms';
import { CodeEditorOptions } from '../../interfaces/code-editor/code-editor-options';
import { Extension } from '@codemirror/state';
import { customDarkTheme } from '../../interfaces/code-editor/custom-dark-theme';

@Component({
  selector: 'gist-viewer',
  standalone: true,
  imports: [FormsModule, DatePipe, CodeEditor],
  templateUrl: './gist-viewer.component.html',
  styleUrl: './gist-viewer.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class GistViewerComponent {
  //* Inputs & Outputs
  public gist = input.required<Gist>();
  @Output() public onFileChanged = new EventEmitter<string>();
  //* State management
  private currentFileIndex = signal<number>(0);
  protected file = computed<GistFile | undefined>(() => {
    return this.gist().files.at(this.currentFileIndex());
  });
  //* GistPicker dynamic dialog
  private gistPickerDialogRef: DynamicDialogRef | undefined;
  private gistPickerDialogConfig = computed<DynamicDialogConfig>(() => {
    return {
      header: 'Gist files',
      width: '30rem',
      modal: true,
      closable: false,
      dismissableMask : true,
      styleClass : 'custom-dialog',
      data : {
        shared : {
          files : this.gist().files,
          activeFileId : this.file()?.id,
        } as GistFilePickerSharedData
      }
    }
  });
  //* Code editor
  public languages = languages;
  public codeEditorOptions = computed<CodeEditorOptions>(
    () => {
      return {
        disabled : false,
        readonly : true,
        theme : 'dark',
        setup: 'basic',
        placeHolder : 'Your code here...',
        language : this.file()?.language,
      } as CodeEditorOptions
    }
  );
  public cmExtensions: Extension[] = [
    customDarkTheme
  ];

  constructor(private dialogService: DialogService) {}
  
  public openGistPickerDialog() { 
    this.gistPickerDialogRef = this.dialogService.open(
      GistFilePickerComponent,
      this.gistPickerDialogConfig(),
    );
    this.handleGistPickerDialogClose();
  }
  
  private handleGistPickerDialogClose(): void {
    this.gistPickerDialogRef?.onClose
    .subscribe((selectedFileId : string) => {
      const fileindex = this.gist().files
        .findIndex(f => f.id === selectedFileId);
      this.currentFileIndex.update(() => fileindex);
      this.onFileChanged.emit(this.file()?.fileName);
    });
  }
}
