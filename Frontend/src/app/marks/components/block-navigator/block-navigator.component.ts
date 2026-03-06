import { ChangeDetectionStrategy, Component, computed, EventEmitter, input, Output } from '@angular/core';
import { TooltipModule } from 'primeng/tooltip';
import { Block } from '../../interfaces/block';

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
  public blocks = input.required<Block[]>();
  @Output() public positionChanged = new EventEmitter<number>();

  protected previousBlockName = computed<string>(() => {
    const currentIndex = this.blockIndex();
    if (currentIndex === 0) return "";
    return this.blocks()[currentIndex - 1].title;
  });

  protected nextBlockName = computed<string>(() => {
    const currentIndex = this.blockIndex();
    const blocks = this.blocks();
    if (currentIndex >= blocks.length - 1) return "";
    return blocks[currentIndex + 1].title;
  });

  public onNavigateBackwardBtnClick(): void {
    if (this.blockIndex() === 0) return;
    const newIndex = this.blockIndex() - 1;
    this.positionChanged.emit(newIndex);
  }

  public onNavigateForwardBtnClick(): void {
    if (this.blockIndex() === this.blocks().length - 1) return;
    const newIndex = this.blockIndex() + 1;
    this.positionChanged.emit(newIndex);
  }
}