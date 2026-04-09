import { ChangeDetectionStrategy, Component, computed, EventEmitter, input, Output, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { CodeEditor } from '@acrodata/code-editor';
import { DynamicDialogRef, DialogService, DynamicDialogConfig } from 'primeng/dynamicdialog';
import { Extension } from '@codemirror/state';
import { InputTextModule } from 'primeng/inputtext';
import { languages } from '@codemirror/language-data';

import { CodeEditorOptions } from '../../interfaces/code-editor/code-editor-options';
import { customDarkTheme } from '../../interfaces/code-editor/custom-dark-theme';
import { customLightTheme } from '../../interfaces/code-editor/custom-light-theme';
import { Gist, GistFile } from '../../interfaces/gist';
import { GistFilePickerComponent, GistFilePickerSharedData } from '../gist-file-picker/gist-file-picker.component';
import { ThemeService } from '../../../../../shared/services/theme.service';

@Component({
    selector: 'gist-viewer',
    imports: [
      CodeEditor,
      DatePipe,
      FormsModule,
      InputTextModule,
    ],
    templateUrl: './gist-viewer.component.html',
    styleUrl: './gist-viewer.component.css',
    changeDetection: ChangeDetectionStrategy.OnPush
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
  private gistPickerDialogRef ?: DynamicDialogRef<GistFilePickerComponent> | null;
  private gistPickerDialogConfig = computed<DynamicDialogConfig>(() => {
    return {
      header: 'Gist files',
      width: '35rem',
      modal: true,
      closable: true,
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
        theme : this.themeService.theme(),
        setup: 'basic',
        placeHolder : 'Your code here...',
        language : this.file()?.language,
      } as CodeEditorOptions
    }
  );
  public codeMirrorExtensions = computed<Extension[]>(() => {
    return this.themeService.theme() === 'dark'
      ? [customDarkTheme]
      : [customLightTheme];
  });

  constructor(
    private dialogService: DialogService,
    private themeService: ThemeService,
  ) {}
  
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
      if (selectedFileId) {
        const fileindex = this.gist().files
          .findIndex(f => f.id === selectedFileId);
        this.currentFileIndex.update(() => fileindex);
        this.onFileChanged.emit(this.file()?.fileName);
      }
    });
  }
}
