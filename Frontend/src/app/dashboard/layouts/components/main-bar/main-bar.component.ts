import { Component, CUSTOM_ELEMENTS_SCHEMA, EventEmitter, Output } from '@angular/core';
import { Router } from '@angular/router';

import { MenuItem } from 'primeng/api';
import { PrimengModule } from './../../../../shared/primeng/primeng.module';

import { AuthService } from './../../../../auth/services/auth.service';

@Component({
  selector: 'app-main-bar',
  templateUrl: './main-bar.component.html',
  standalone: true,
  imports: [
    PrimengModule
  ],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  styleUrl: './main-bar.component.css'
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
  @Output()
  public onHideSideBar = new EventEmitter<boolean>();

  private navigate( path: string ): void {
    this.router.navigate([path]);
    this.onHideSideBar.emit(true);
  }
}
