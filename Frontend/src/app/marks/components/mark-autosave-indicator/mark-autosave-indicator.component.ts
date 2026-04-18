import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, EventEmitter, input, Output } from '@angular/core';

export enum SaveState {
  idle,
  saving,
  saved,
  error
}

@Component({
    selector: 'mark-autosave-indicator',
    imports: [
        CommonModule
    ],
    templateUrl: './mark-autosave-indicator.component.html',
    styleUrl: './mark-autosave-indicator.component.css',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class MarkAutosaveIndicatorComponent {
  public saveState = input.required<SaveState>();
  @Output() public retrySave = new EventEmitter<void>();
  get saveStateType(): typeof SaveState {
    return SaveState;
  }
}
