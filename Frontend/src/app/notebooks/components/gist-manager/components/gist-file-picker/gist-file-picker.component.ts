import { ChangeDetectionStrategy, Component } from '@angular/core';
import { NgClass } from '@angular/common';

import { DynamicDialogConfig, DynamicDialogRef } from 'primeng/dynamicdialog';

import { GistFile } from '../../interfaces/gist';

export interface GistFilePickerSharedData {
  files : GistFile[],
  activeFileId : string
}

@Component({
    selector: 'app-gist-file-picker',
    imports: [NgClass],
    templateUrl: './gist-file-picker.component.html',
    styleUrl: './gist-file-picker.component.css',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class GistFilePickerComponent {
  protected files: GistFile[];
  protected selectedFileId : string;

  constructor(
    private config: DynamicDialogConfig,
    private ref: DynamicDialogRef,
  ) {
    const data = this.config.data.shared as GistFilePickerSharedData;
    this.validateSharedData(data);
    const { files, activeFileId } = data;
    this.files = files;
    this.selectedFileId = activeFileId;
  }

  public onFileSelected(fileId : string): void {
    this.ref.close(fileId);
  }

  public onCancel(): void {
    this.ref.close();
  }
  
  private validateSharedData(data : GistFilePickerSharedData): void {
    if (!data) {
      throw new Error('Required data is missing.');
    }

    if (!Array.isArray(data.files) || data.files.length === 0) {
      this.handleMissingDataError('files (must be a non-empty array)');
    }

    if (!data.activeFileId || data.activeFileId.trim().length === 0)
      this.handleMissingDataError('activeFile');
  }

  private handleMissingDataError( requiredDataName: string ): void {
    this.ref.destroy();
    throw new Error(`The '${ requiredDataName }' shared data is required.`);
  }
}
