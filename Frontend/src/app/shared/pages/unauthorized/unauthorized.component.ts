import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ROUTES } from '../../utils/constant';

@Component({
    selector: 'app-unauthorized',
    imports: [RouterLink],
    template: `
    <div class="error-container">
      <div class="error-content shadow-4">
        <h1>401</h1>
        <h2>Unauthorized Access</h2>
        <p>You don't have permission to access this resource.</p>
        <button [routerLink]="HOME_ROUTE">
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

    button {
      cursor: pointer;  
      font-size: 1rem;
      min-width: 200px;
      padding: .5rem;
      color: var(--text-secondary);
      border-radius: .25rem;
      border: 1px solid var(--p-highlight-primary-border);
      background-color: var(--p-highlight-primary-background);
      transition: 0.3s all ease;
    }
    button:hover {
      color: var(--text-primary);
    }
  `]
})
export class UnauthorizedComponent {
  protected readonly HOME_ROUTE : string = ROUTES.HOME_URL;
}
