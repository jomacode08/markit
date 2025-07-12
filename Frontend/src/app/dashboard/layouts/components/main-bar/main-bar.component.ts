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
      icon: 'fa fa-home',
      command: () => this.navigate(ROUTES.HOME_URL)
    },
    {
      label: 'My marks',
      icon: 'fa fa-hard-drive',
      command: () => this.navigate(ROUTES.MY_MARKS)
    },
    {
      label: 'Starred',
      icon: 'fa fa-star',
      command: () => this.navigate(ROUTES.STARRED)
    },
    {
      label: 'Recent',
      icon: 'fa fa-clock',
      command: () => this.navigate(ROUTES.RECENT)
    },
  ];

  public userActions : MenuItem[] = [
    {
      label: 'Profile',
      icon: 'fa fa-user',
      command: () => this.navigate('dashboard/profile')
     },
    {
      label: 'Logout',
      icon: 'fa fa-right-to-bracket',
      command: () => this.authService.logout()
    },
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

  private navigate( path: string ): void {
    this.router.navigate([path]);
  }
}
