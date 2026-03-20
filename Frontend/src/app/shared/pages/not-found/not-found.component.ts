import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';

@Component({
    selector: 'app-not-found',
    imports: [ButtonModule, RouterLink],
    template: `
    <div class="error-container">
      <div class="error-content shadow-4">
        <h1>404</h1>
        <h2>Page Not Found</h2>
        <p>The page you are looking for doesn't exist or has been moved.</p>
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
      background: #009FFF;
      background: -webkit-linear-gradient(to right, #ec2F4B, #009FFF);
      background: linear-gradient(to right, #ec2F4B, #009FFF);
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
export class NotFoundComponent {}
