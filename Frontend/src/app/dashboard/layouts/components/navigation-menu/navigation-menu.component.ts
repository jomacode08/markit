import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, input } from '@angular/core';

import { MenuItem } from 'primeng/api';
import { MenuModule } from 'primeng/menu';
import { AuthService } from '../../../../auth/services/auth.service';

@Component({
  selector: 'app-navigation-menu',
  standalone: true,
  imports: [
    CommonModule,
    MenuModule,
  ],
  templateUrl: './navigation-menu.component.html',
  styleUrl: './navigation-menu.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NavigationMenuComponent {
  public navigationItems = input.required<MenuItem[]>();
  public userName ?: string;
  public userPictureUrl ?: string;

  constructor(private authService: AuthService) {
    this.userName = authService.currentUser()?.givenName;
    this.userPictureUrl = authService.currentUser()?.userPictureUrl;
  }

  public onLogoutBtnClick = () => this.authService.logout();
}
