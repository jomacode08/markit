import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';

@Component({
    selector: 'app-unauthorized',
    imports: [ButtonModule, RouterLink],
    template: `
    <div class="error-container">
      <div class="error-content shadow-4">
        <h1>401</h1>
        <h2>Unauthorized Access</h2>
        <p>You don't have permission to access this resource.</p>
        <p-button
          styleClass="w-6"
          type="button"
          severity="primary"
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
      border: 1px solid var(--surface-border);
      border-radius: 0.5rem;
    }

    h1 {
      font-size: 6rem !important;
      margin: 0;
      background: #654ea3;
      background: -webkit-linear-gradient(to right, #eaafc8, #654ea3);
      background: linear-gradient(to right, #eaafc8, #654ea3);
      -webkit-background-clip: text;
      -webkit-text-fill-color: transparent;
      font-weight: bold;
    }

    h2 {
      font-size: 2rem !important;
      margin: 1rem 0;
      color: var(--text-primary);
    }

    p {
        color: var(--text-secondary);
        margin-bottom: 2rem;
    }
  `]
})
export class UnauthorizedComponent {}
