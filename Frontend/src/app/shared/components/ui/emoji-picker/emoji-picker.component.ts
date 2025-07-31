import { AfterViewInit, ChangeDetectionStrategy, Component, CUSTOM_ELEMENTS_SCHEMA, ElementRef, EventEmitter, input, Output, ViewChild } from '@angular/core';
import { OverlayPanel, OverlayPanelModule } from 'primeng/overlaypanel';
import 'emoji-picker-element';
import { Picker } from 'emoji-picker-element';
import { EmojiClickEvent } from 'emoji-picker-element/shared';

@Component({
  selector: 'app-emoji-picker',
  standalone: true,
  imports: [OverlayPanelModule],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  styleUrl: './emoji-picker.component.css',
  template: `
  <!-- Picker button -->
  <button type="button" aria-label="emoji picker" (click)="panel.toggle($event)">
    @if (currentEmoji()) {
      <span id="emoji" class="text-2xl">{{ currentEmoji() }}</span>
    }
    @else {
      <span id="defaultIcon" [class]="defaultIconClass() ?? 'fa-regular fa-smile'"></span>
    }
  </button>
  <!-- Overlay panel -->
  <p-overlayPanel #panel>
    <div class="picker-header">
      <button type="button" aria-label="Delete emoji" (click)="onDeleteEmojiBtnClick()">
        Delete
      </button>
    </div>
    <emoji-picker #emojiPicker></emoji-picker>
  </p-overlayPanel>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EmojiPickerComponent implements AfterViewInit {
  @ViewChild('panel') private panelRef!: OverlayPanel;
  @ViewChild('emojiPicker', { static: false }) private emojiPickerRef!: ElementRef;
  @Output() public emojiClick = new EventEmitter<string>();
  @Output() public emojiDeleted = new EventEmitter<boolean>();
  public currentEmoji = input<string | undefined>(undefined);
  public defaultIconClass = input<string | undefined>(undefined);
  
  public ngAfterViewInit(): void {
    if (!this.panelRef) throw new Error('The overlay-panel component is not implemented');
    if (!this.emojiPickerRef) throw new Error('The emoji-picker component is not implemented');
    const picker : Picker = this.emojiPickerRef.nativeElement;
    picker.addEventListener(
      'emoji-click', (event: any) => {
        console.log(event);
        this.emojiClick.emit(event.detail.emoji.unicode);
        this.panelRef.toggle(new Event('click'));
      }
    );
  }

  public onDeleteEmojiBtnClick() {
    this.emojiDeleted.emit(true);
  }
}
