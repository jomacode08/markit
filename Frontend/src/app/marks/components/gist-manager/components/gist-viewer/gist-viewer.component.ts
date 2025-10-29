import { ChangeDetectionStrategy, Component, computed, input, signal } from '@angular/core';
import { DatePipe, JsonPipe } from '@angular/common';

import { DynamicDialogRef, DialogService, DynamicDialogConfig } from 'primeng/dynamicdialog';

import { Gist } from '../../interfaces/gist';
import { GistFilePickerComponent, GistFilePickerSharedData } from '../gist-file-picker/gist-file-picker.component';

@Component({
  selector: 'gist-viewer',
  standalone: true,
  imports: [JsonPipe, DatePipe],
  templateUrl: './gist-viewer.component.html',
  styleUrl: './gist-viewer.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class GistViewerComponent {
  public gist = input.required<Gist>();
  protected currentFileIndex = signal<number>(0);
  //* GistPicker dynamic dialog configuration
  private gistPickerDialogRef: DynamicDialogRef | undefined;
  private gistPickerDialogConfig = computed<DynamicDialogConfig>(() => {
    return {
      header: 'Gist Files',
      width: '30rem',
      modal: true,
      closable: false,
      dismissableMask : true,
      styleClass : 'custom-dialog',
      data : {
        shared : {
          files : this.gist().files,
          fileSelectedIndex : this.currentFileIndex(),
        } as GistFilePickerSharedData
      }
    }
  });

  constructor(private dialogService: DialogService) {}
  
  public openGistPickerDialog() { 
    this.gistPickerDialogRef = this.dialogService.open(
      GistFilePickerComponent,
      this.gistPickerDialogConfig(),
    );
  
    this.gistPickerDialogRef.onClose
    .subscribe((selectedFileIndex : number) => {
      if (this.isValidIndex(selectedFileIndex)) {
        this.currentFileIndex.update(() => selectedFileIndex);
      }
    });
  }

  private isValidIndex = (index: number): boolean => typeof index === 'number'
    && !isNaN(index)
    && index >= 0
    && index < this.gist().files.length;
}
