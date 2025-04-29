import { Component, CUSTOM_ELEMENTS_SCHEMA, EventEmitter, Output } from '@angular/core';
import { Router } from '@angular/router';

import { ButtonModule } from 'primeng/button';
import { MenuItem } from 'primeng/api';

import { AuthService } from './../../../../auth/services/auth.service';
import { GeneralConstant } from '../../../../shared/utils/general-constant';
import { ROUTES } from '../../../../shared/interfaces/constant';

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
      command: () => this.navigate(GeneralConstant.HOME_URL)
    },
    {
      label: 'My marks',
      icon: 'fa fa-file',
      command: () => this.navigate('marks')
    },
    {
      label: 'My collections',
      icon: 'fa fa-folder',
      command: () => this.navigate('collections')
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
  public onShowSideBar = new EventEmitter<MenuItem[]>();

  public onNewMarkButtonClick = () => this.navigate(ROUTES.MARKS_NEW);

  private navigate( path: string ): void {
    this.router.navigate([path]);
  }
}
