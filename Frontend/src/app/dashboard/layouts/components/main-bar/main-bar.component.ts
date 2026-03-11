import { Component, CUSTOM_ELEMENTS_SCHEMA, EventEmitter, Output } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';

import { ButtonModule } from 'primeng/button';
import { MenuItem } from 'primeng/api';
import { TooltipModule } from 'primeng/tooltip';

import { AuthService } from './../../../../auth/services/auth.service';
import { ROUTES } from '../../../../shared/utils/constant';
import { ProfileMenu } from '../profile-menu/profile-menu.component';

@Component({
    selector: 'app-main-bar',
    templateUrl: './main-bar.component.html',
    imports: [
        ButtonModule,
        RouterLink,
        TooltipModule,
        RouterLinkActive,
        ProfileMenu,
    ],
    schemas: [CUSTOM_ELEMENTS_SCHEMA],
    styleUrl: './main-bar.component.css'
})
export class MainBarComponent {
  public readonly MAIN_BAR_BRAND_TITLE: string = "Mark it";
  
  public navigationRutes : MenuItem[] = [
    {
      label: 'Dashboard',
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

  public onAvatarClick = () => this.navigate(ROUTES.PROFILE);

  private navigate( path: string ): void {
    this.router.navigate([path]);
  }
}