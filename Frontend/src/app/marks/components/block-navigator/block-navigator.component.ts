import { ChangeDetectionStrategy, Component, EventEmitter, input, Output } from '@angular/core';
import { TooltipModule } from 'primeng/tooltip';

@Component({
  selector: 'block-navigator',
  standalone: true,
  imports: [TooltipModule],
  templateUrl: './block-navigator.component.html',
  styleUrl: './block-navigator.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BlockNavigatorComponent {
  public blockIndex = input.required<number>();
  public blocksLength = input.required<number>();
  @Output() public positionChanged = new EventEmitter<number>();

  public onNavigateBackwardBtnClick(): void {
    if (this.blockIndex() === 0) return;
    const newIndex = this.blockIndex() - 1;
    this.positionChanged.emit(newIndex);
  }

  public onNavigateForwardBtnClick(): void {
    if (this.blockIndex() === this.blocksLength() - 1) return;
    const newIndex = this.blockIndex() + 1;
    this.positionChanged.emit(newIndex);
  }
}