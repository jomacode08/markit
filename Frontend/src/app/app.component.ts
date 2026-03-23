import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

import { AlertMessageComponent } from './shared/components/layout/alert-message/alert-message.component';

@Component({
    selector: 'app-root',
    imports: [
      AlertMessageComponent,
      RouterOutlet,
    ],
    templateUrl: './app.component.html',
    styleUrl: './app.component.css'
})
export class AppComponent {
  title = 'markit-app';
  constructor() {}
}
