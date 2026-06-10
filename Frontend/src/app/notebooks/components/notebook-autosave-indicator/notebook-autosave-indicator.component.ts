import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, EventEmitter, input, Output } from '@angular/core';

export enum SaveState {
  idle,
  saving,
  saved,
  error
}

@Component({
    selector: 'notebook-autosave-indicator',
    imports: [
        CommonModule
    ],
    templateUrl: './notebook-autosave-indicator.component.html',
    styleUrl: './notebook-autosave-indicator.component.css',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class NotebookAutosaveIndicatorComponent {
  public saveState = input.required<SaveState>();
  @Output() public retrySave = new EventEmitter<void>();
  get saveStateType(): typeof SaveState {
    return SaveState;
  }
}
