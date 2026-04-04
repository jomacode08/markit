import { ChangeDetectionStrategy, Component, computed, inject, Renderer2, ViewChild, DOCUMENT } from '@angular/core';
import { RouterModule } from '@angular/router';

import { MenuItem } from 'primeng/api';
import { MenuModule } from 'primeng/menu';
import { TieredMenu, TieredMenuModule } from 'primeng/tieredmenu';

import { AuthRole } from '../../../../auth/interfaces/auth-role.enum';
import { AuthService } from '../../../../auth/services/auth.service';
import { ROUTES } from '../../../../shared/utils/constant';
import { ThemeService } from '../../../../shared/services/theme.service';

@Component({
    selector: 'app-profile-menu',
    imports: [
      MenuModule,
      RouterModule,
      TieredMenuModule,
    ],
    template: `
    <p-tiered-menu
      #profileMenu
      styleClass="popup"
      appendTo="body"
      [model]="profileMenuItems()"
      [popup]="true"
      (onShow)="disableScroll()"
      (onHide)="enableScroll()"
    />
  `,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProfileMenu {
  private authService = inject(AuthService);
  private themeService = inject(ThemeService);
  private renderer = inject(Renderer2);
  private document = inject(DOCUMENT);

  @ViewChild('profileMenu') private profileMenu !: TieredMenu;
  private readonly STOP_SCROLLING_CLASS_NAME = 'stop-scrolling';
  protected profileMenuItems = computed<MenuItem[]>(() => [
    {
      label: 'Settings',
      icon: 'fa-solid fa-sliders',
      routerLink: ROUTES.ACCOUNTS,
      visible: this.authService.currentUser()?.roles.includes(AuthRole.ADMIN) ?? false,
    },
    {
      label: 'Profile',
      icon: 'fa-regular fa-user',
      routerLink: ROUTES.PROFILE,
    },
    {
      label: 'Theme',
      icon: 'fa-solid fa-circle-half-stroke',
      items: [
        {
          label: 'Light',
          icon: 'fa-regular fa-sun',
          command: () => this.themeService.setLightMode()
        },
        {
          label: 'Dark',
          icon: 'fa-regular fa-moon',
          command: () => this.themeService.setDarkMode()
        }
      ]
    },
    {
      label: 'Sign Out',
      icon: 'fa fa-right-to-bracket',
      command: () => {
        this.authService.logout();
      }
    },
  ]);

  public toggle(event: Event) {
    this.profileMenu.toggle(event);
  }

  public disableScroll() {
    this.renderer.addClass(
      this.document.body,
      this.STOP_SCROLLING_CLASS_NAME
    );
  }

  public enableScroll() {
    this.renderer.removeClass(
      this.document.body,
      this.STOP_SCROLLING_CLASS_NAME
    );
  }
}