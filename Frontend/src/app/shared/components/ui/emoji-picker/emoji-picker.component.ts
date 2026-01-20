import { AfterViewInit, ChangeDetectionStrategy, Component, CUSTOM_ELEMENTS_SCHEMA, ElementRef, forwardRef, Input, input, signal, ViewChild } from '@angular/core';
import { NgClass } from '@angular/common';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

import { OverlayPanel, OverlayPanelModule } from 'primeng/overlaypanel';
import 'emoji-picker-element';

import { Picker } from 'emoji-picker-element';

@Component({
  selector: 'app-emoji-picker',
  standalone: true,
  imports: [OverlayPanelModule, NgClass],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => EmojiPickerComponent),
      multi: true
    }
  ],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  styleUrl: './emoji-picker.component.css',
  template: `
  <!-- Picker button -->
  <button
    type="button"
    aria-label="emoji picker"
    (click)="onPickerButtonClick($event)"
    [ngClass]="{ 'opacity-40' : disabled}"
    [disabled]="disabled"
  >
    @if (currentEmoji()) {
      <span id="emoji">{{ currentEmoji() }}</span>
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
export class EmojiPickerComponent implements ControlValueAccessor, AfterViewInit {
  @ViewChild('panel') private panelRef!: OverlayPanel;
  @ViewChild('emojiPicker', { static: false }) private emojiPickerRef!: ElementRef;
  @Input() public disabled : boolean = false;
  public defaultIconClass = input<string | undefined>(undefined);
  public currentEmoji = signal<string | undefined>(undefined);
  
  public ngAfterViewInit(): void {
    if (!this.panelRef) throw new Error('The overlay-panel component is not implemented');
    if (!this.emojiPickerRef) throw new Error('The emoji-picker component is not implemented');
    const picker : Picker = this.emojiPickerRef.nativeElement;
    picker.addEventListener(
      'emoji-click', (event: any) => {
        this.writeValue(event.detail.emoji.unicode);
        this.panelRef.toggle(new Event('click'));
      }
    );
  }
  // Function to call when the emojji changes.
  onChange = (emoji: string | undefined) => {};
  // Function to call when an emoji is selected.
  onTouched = () => {};
  // Allow Angular to update the model (emoji).
  writeValue(emoji: string | undefined): void {
    this.currentEmoji.update(() => emoji);
    this.onChange(emoji);
  }
  // Allows Angular to register a function to call when the model (emoji) changes.
  registerOnChange(fn: (emoji : string | undefined) => void): void {
    this.onChange = fn;
  }
  // Allows Angular to register a function to call when the input has been touched.
  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }
  // Allows Angular to disable the input
  setDisabledState?(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }

  public onPickerButtonClick(event: Event) {
    this.onTouched();
    this.panelRef.toggle(event);
  }

  public onDeleteEmojiBtnClick() {
    this.writeValue(undefined);
  }
}
