import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
    selector: 'app-not-found',
    imports: [RouterLink],
    template: `
    <div class="error-container">
      <div class="error-content">
        <h1>404</h1>
        <h2>Page Not Found</h2>
        <p>The page you are looking for doesn't exist or has been moved.</p>
        <button routerLink="/">
          Go to Home
        </button>
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
      background-color: var(--paper-bg);
    }

    .error-content {
      padding: 2rem;
      background-color: var(--surface-2);
      border: 1px solid var(--surface-border);
      border-radius: 0.5rem;
      box-shadow: 0 4px 6px var(--shadow-color);
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

    button {
      cursor: pointer;  
      font-size: 1rem;
      min-width: 200px;
      padding: .5rem;
      color: var(--text-secondary);
      border-radius: .25rem;
      border: 1px solid var(--highlight-border-purple);
      background-color: var(--highlight-surface-purple);
      transition: 0.3s all ease;
    }
    button:hover {
      color: var(--text-primary);
    }
  `]
})
export class NotFoundComponent {}
