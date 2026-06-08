import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'shared-pixel-notepad',
  imports: [],
  template: `
    <div class="noty-container">
      <svg
        class="noty-svg"
        xmlns="http://www.w3.org/2000/svg"
        viewBox="0 0 32 32"
        role="img"
        aria-label="Notepad illustration"
      >
        <g>
          <path d="m12.955 0 0 1.52 -3.05 0 0 1.53 3.05 0 0 1.52 1.52 0 0 -1.52 3.05 0 0 1.52 1.52 0 0 -1.52 3.05 0 0 1.52 1.53 0 0 -1.52 1.52 0 0 3.05 1.52 0 0 -1.53 1.53 0 0 25.91 -16.76 0 0 1.52 18.28 0 0 -1.52 1.52 0 0 -25.91 -1.52 0 0 -1.52 -1.52 0 0 -1.53 -4.57 0 0 -1.52 -1.53 0 0 1.52 -3.05 0 0 -1.52 -1.52 0 0 1.52 -3.05 0 0 -1.52 -1.52 0z" fill="currentColor"/>
          <path d="M23.625 6.1h1.52v3.04h-1.52Z" fill="currentColor"/>
          <path d="m22.095 12.19 4.57 0 0 -1.52 -3.04 0 0 -1.53 -1.53 0 0 3.05z" fill="currentColor"/>
          <path d="m20.575 15.24 -1.53 0 0 3.05 1.53 0 0 -1.53 6.09 0 0 -1.52 -4.57 0 0 -3.05 -1.52 0 0 3.05z" fill="currentColor"/>
          <path d="m17.525 21.33 9.14 0 0 -1.52 -7.62 0 0 -1.52 -1.52 0 0 3.04z" fill="currentColor"/>
          <path d="M16.005 12.19h1.52v1.53h-1.52Z" fill="currentColor"/>
          <path d="M16.005 9.14h1.52v1.53h-1.52Z" fill="currentColor"/>
          <path d="M12.955 24.38h13.71v1.53h-13.71Z" fill="currentColor"/>
          <path d="M11.435 13.72h4.57v1.52h-4.57Z" fill="currentColor"/>
          <path d="M11.435 9.14h1.52v1.53h-1.52Z" fill="currentColor"/>
          <path d="m11.435 22.86 6.09 0 0 -1.53 -15.24 0 0 1.53 7.62 0 0 7.62 1.53 0 0 -7.62z" fill="currentColor"/>
          <path d="M9.905 12.19h1.53v1.53h-1.53Z" fill="currentColor"/>
          <path d="M8.385 3.05h1.52V6.1h-1.52Z" fill="currentColor"/>
          <path d="M6.855 6.1h1.53v3.04h-1.53Z" fill="currentColor"/>
          <path d="M5.335 9.14h1.52v3.05h-1.52Z" fill="currentColor"/>
          <path d="M3.815 12.19h1.52v3.05h-1.52Z" fill="currentColor"/>
          <path d="M2.285 15.24h1.53v3.05h-1.53Z" fill="currentColor"/>
          <path d="M0.765 18.29h1.52v3.04H0.765Z" fill="currentColor"/>
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
