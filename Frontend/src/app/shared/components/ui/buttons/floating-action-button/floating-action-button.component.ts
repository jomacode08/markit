import { ChangeDetectionStrategy, Component, EventEmitter, input, Output } from '@angular/core';

@Component({
  selector: 'shared-floating-action-button',
  standalone: true,
  imports: [],
  styleUrl: './floating-action-button.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="floating-button">
      <button type="button" [disabled]="!enabled()" (click)="onButtonClicked()">
        <span [class]="iconClass()"></span>
      </button>
    </div>
  `,
})
export class FloatingActionButtonComponent {
  public iconClass = input.required<string>();
  public enabled = input<boolean>(true);
  @Output() public onClick = new EventEmitter<void>();

  public onButtonClicked = () => this.onClick.emit();
}