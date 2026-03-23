import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';

@Component({
    selector: 'app-error',
    imports: [ButtonModule, RouterLink],
    template: `
    <div class="error-container">
      <div class="error-content shadow-4">
        <h1>500</h1>
        <h2>Something Went Wrong</h2>
        <p>We're experiencing some technical difficulties. Please try again later.</p>
        <p-button
          styleClass="w-6"
          type="button"
          severity="danger"
          size="small"
          label="Go to home"
          ariaLabel="Go to home"
          routerLink="/"
        />
      </div>
    </div>
  `,
    styles: [`
    .error-container {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      height: 100vh;
      text-align: center;
      background-color: var(--p-surface-950);
    }

    .error-content {
      padding: 2rem;
      background-color: var(--p-surface-800);
      border: 1px solid var(--p-overlay-modal-border-color);
      border-radius: 0.5rem;
    }

    h1 {
      font-size: 6rem !important;
      margin: 0;
      background: #8A2387;
      background: -webkit-linear-gradient(to right, #F27121, #E94057, #8A2387);
      background: linear-gradient(to right, #F27121, #E94057, #8A2387);
      -webkit-background-clip: text;
      -webkit-text-fill-color: transparent;
      font-weight: bold;
    }

    h2 {
      font-size: 2rem !important;
      margin: 1rem 0;
      color: var(--p-text-color);
    }

    p {
      color: var(--p-text-hover-color);
      margin-bottom: 2rem;
    }

    .button-group {
      display: flex;
      gap: 1rem;
      justify-content: center;
    }

    .general-button {
      min-width: 150px;
    }
  `]
})
export class ErrorComponent {}
