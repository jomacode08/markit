import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'shared-pixel-notepad',
  imports: [],
  template: `
    <div class="noty-container">
      <svg class="noty-svg" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 32 32">
        <title>computer-old-electronics</title>
        <g>
          <path d="M30.475 22.86h-4.57v-1.53h1.52v-1.52H4.575v1.52H6.1v1.53H1.525V32h28.95Zm-22.86 -1.53h16.76v1.53H7.615Zm21.34 9.15H3.045v-6.1h25.91Z" fill="currentColor" stroke-width="1"></path>
          <path d="M27.425 1.52h1.53v18.29h-1.53Z" fill="currentColor" stroke-width="1"></path>
          <path d="M18.285 25.9h7.62v1.53h-7.62Z" fill="currentColor" stroke-width="1"></path>
          <path d="M18.285 10.67h1.52v1.52h-1.52Z" fill="currentColor" stroke-width="1"></path>
          <path d="M18.285 7.62h1.52v1.52h-1.52Z" fill="currentColor" stroke-width="1"></path>
          <path d="M13.715 12.19h4.57v1.52h-4.57Z" fill="currentColor" stroke-width="1"></path>
          <path d="M12.185 10.67h1.53v1.52h-1.53Z" fill="currentColor" stroke-width="1"></path>
          <path d="M12.185 7.62h1.53v1.52h-1.53Z" fill="currentColor" stroke-width="1"></path>
          <path d="M6.1 18.29h19.81V3.05H6.1ZM7.615 4.57h16.76v12.19H7.615Z" fill="currentColor" stroke-width="1"></path>
          <path d="M6.095 25.9h3.05v3.05h-3.05Z" fill="currentColor" stroke-width="1"></path>
          <path d="M4.575 0h22.85v1.52H4.575Z" fill="currentColor" stroke-width="1"></path>
          <path d="M3.045 1.52h1.53v18.29h-1.53Z" fill="currentColor" stroke-width="1"></path>
        </g>
      </svg>
    </div>
  `,
  styles: [`
    .noty-container {
      display: flex;
      align-items: center;
      justify-content: center;
      width: 80px;
      min-width: 80px;
      height: 80px;
      border: 1px solid var(--p-content-border-color);
      border-radius: 8px;
      overflow: hidden;
      padding: .875rem;
      background-color: var(--p-surface-0);
      background-image:
        repeating-linear-gradient(0deg, transparent, transparent 5px, rgba(75, 85, 99, 0.06) 5px, rgba(75, 85, 99, 0.06) 6px, transparent 6px, transparent 15px),
        repeating-linear-gradient(90deg, transparent, transparent 5px, rgba(75, 85, 99, 0.06) 5px, rgba(75, 85, 99, 0.06) 6px, transparent 6px, transparent 15px),
        repeating-linear-gradient(0deg, transparent, transparent 10px, rgba(107, 114, 128, 0.04) 10px, rgba(107, 114, 128, 0.04) 11px, transparent 11px, transparent 30px),
        repeating-linear-gradient(90deg, transparent, transparent 10px, rgba(107, 114, 128, 0.04) 10px, rgba(107, 114, 128, 0.04) 11px, transparent 11px, transparent 30px);
    }

    .noty-svg {
      display: block;
      width: 100%;
      height: 100%;
      color: var(--p-text-hover-color);
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PixelNotepadComponent {}
