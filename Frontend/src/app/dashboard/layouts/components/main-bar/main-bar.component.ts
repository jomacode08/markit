import { PrimengModule } from './../../../../shared/primeng/primeng.module';
import { AuthService } from './../../../../auth/services/auth.service';
import { Component, CUSTOM_ELEMENTS_SCHEMA, EventEmitter, Output } from '@angular/core';
import { MenuItem } from 'primeng/api';

@Component({
  selector: 'app-main-bar',
  templateUrl: './main-bar.component.html',
  standalone: true,
  imports: [
    PrimengModule
  ],
  schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class MainBarComponent {
  public navigationRutes : MenuItem[] = [
    {
      label: 'Test 1',
      icon: 'fa fa-user'
    },
    {
      label: 'Test2',
      icon: 'fa fa-right-to-bracket'
    },
  ];

  public userActions : MenuItem[] = [
    {
      label: 'Profile',
      icon: 'fa fa-user'
    },
    {
      label: 'Logout',
      icon: 'fa fa-right-to-bracket',
      command: () => this.authService.logout()
    },
  ];

  public userPictureUrl ?: string;

  constructor(private authService : AuthService) {
    this.userPictureUrl = authService.currentUser()?.userPictureUrl;
  }

  @Output()
  public onNavigationAction = new EventEmitter<MenuItem[]>();
}
