import { Component, CUSTOM_ELEMENTS_SCHEMA, EventEmitter, Output } from '@angular/core';
import { Router } from '@angular/router';

import { ButtonModule } from 'primeng/button';
import { MenuItem } from 'primeng/api';

import { AuthService } from './../../../../auth/services/auth.service';
import { ROUTES } from '../../../../shared/utils/constant';

@Component({
  selector: 'app-main-bar',
  templateUrl: './main-bar.component.html',
  standalone: true,
  imports: [
    ButtonModule
  ],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  styleUrl: './main-bar.component.css'
})
export class MainBarComponent {
  public readonly MAIN_BAR_BRAND_TITLE: string = "Mark it";
  
  public navigationRutes : MenuItem[] = [
    {
      label: 'Home',
      icon: 'fa-regular fa-chart-bar',
      route: ROUTES.HOME_URL,
    },
    {
      label: 'My marks',
      icon: 'fa-regular fa-hard-drive',
      route: ROUTES.MY_MARKS,
    },
    {
      label: 'Starred',
      icon: 'fa-regular fa-star',
      route: ROUTES.STARRED,
    },
    {
      label: 'Recent',
      icon: 'fa-regular fa-clock',
      route: ROUTES.RECENT,
    }
  ];

  public userPictureUrl ?: string;

  constructor(
    private authService : AuthService,
    private router: Router
  ) {
    this.userPictureUrl = authService.currentUser()?.userPictureUrl;
  }

  @Output()
  public onShowNavigationSidebar = new EventEmitter<MenuItem[]>();
  @Output()
  public onShowSearchSidebar = new EventEmitter<void>();

  public onNewMarkButtonClick = () => this.navigate(ROUTES.MARKS_NEW);
  public onAvatarClick = () => this.navigate(ROUTES.PROFILE);

  private navigate( path: string ): void {
    this.router.navigate([path]);
  }
}
