import { NgTemplateOutlet } from '@angular/common';
import { Component, Input } from '@angular/core';
import { RouterLink } from '@angular/router';

import { ROUTES } from '../../../utils/constant';

@Component({
  selector: 'app-logo',
  imports: [NgTemplateOutlet, RouterLink],
  templateUrl: './app-logo.component.html',
  styleUrl: './app-logo.component.css',
})
export class AppLogoComponent {
  @Input()
  public canRedirect: boolean = false;

  public homeUrl: string = ROUTES.HOME_URL;
}
