import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

import lottie from "lottie-web";
import { defineElement } from "@lordicon/element";

import { AlertMessageComponent } from './shared/components/layout/alert-message/alert-message.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    AlertMessageComponent,
    RouterOutlet,
  ],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  title = 'markit-app';
  constructor() {
    defineElement(lottie.loadAnimation);
  }
}
