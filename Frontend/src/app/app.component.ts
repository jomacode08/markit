import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

import lottie from "lottie-web";
import { defineElement } from "@lordicon/element";
import { SharedModule } from "./shared/shared.module";

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    RouterOutlet,
    SharedModule
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
