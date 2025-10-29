import { ChangeDetectionStrategy, Component } from '@angular/core';
import { NgClass } from '@angular/common';

import { DynamicDialogConfig, DynamicDialogRef } from 'primeng/dynamicdialog';

import { GistFile } from '../../interfaces/gist';

export interface GistFilePickerSharedData {
  files : GistFile[],
  fileSelectedIndex : number;
}

@Component({
  selector: 'app-gist-file-picker',
  standalone: true,
  imports: [NgClass],
  templateUrl: './gist-file-picker.component.html',
  styleUrl: './gist-file-picker.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class GistFilePickerComponent {
  protected files: GistFile[];
  protected fileSelectedIndex : number;

  constructor(
      private config: DynamicDialogConfig,
      private ref: DynamicDialogRef
  ) {
    const data = this.config.data.shared as GistFilePickerSharedData;
    this.validateSharedData(data);
    const { files, fileSelectedIndex } = data;
    this.files = files;
    this.fileSelectedIndex = fileSelectedIndex;
  }

  public onFileSelected(index: number): void {
    this.ref.close(index);
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

    const isValidIndex = typeof data.fileSelectedIndex === 'number' 
      && !isNaN(data.fileSelectedIndex)
      && data.fileSelectedIndex >= 0 
      && data.fileSelectedIndex < data.files.length;

    if (!isValidIndex) {
      this.handleMissingDataError(`fileSelectedIndex (must be between 0 and ${data.files.length - 1})`);
    }
  }

  private handleMissingDataError( requiredDataName: string ): void {
    this.ref.destroy();
    throw new Error(`The '${ requiredDataName }' shared data is required.`);
  }
}
