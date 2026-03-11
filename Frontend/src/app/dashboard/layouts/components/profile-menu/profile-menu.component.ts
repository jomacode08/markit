import { ChangeDetectionStrategy, Component, computed, inject, Inject, Renderer2, ViewChild } from '@angular/core';
import { DOCUMENT, NgIf } from '@angular/common';

import { Menu, MenuModule } from 'primeng/menu';

import { ROUTES } from '../../../../shared/utils/constant';
import { AuthService } from '../../../../auth/services/auth.service';
import { AuthRole } from '../../../../auth/interfaces/auth-role.enum';
import { RouterModule } from '@angular/router';

@Component({
    selector: 'app-profile-menu',
    imports: [
        MenuModule,
        NgIf,
        RouterModule
    ],
    template: `
    <p-menu
      #profileMenu
      styleClass="popup"
      appendTo="body"
      [model]="profileMenuItems()"
      [popup]="true"
      (onShow)="disableScroll()"
      (onHide)="enableScroll()"
    >
      <ng-template pTemplate="item" let-item>
        <ng-container *ngIf="item.route; else elseBlock">
          <a [routerLink]="item.route" class="p-menuitem-link">
            <i [class]="item.icon"></i>
            <span class="ml-3 p-menuitem-text">{{ item.label }}</span>
          </a>
        </ng-container>
        <ng-template #elseBlock>
          <button
            type="button"
            class="p-menuitem-link border-none"
            [attr.aria-label]="item.label"
            (click)="item.command"
          >
            <i [class]="item.icon"></i>
            <span class="ml-3 p-menuitem-text">{{ item.label }}</span>
          </button>
        </ng-template>
      </ng-template>
    </p-menu>
  `,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProfileMenu {
  private authService = inject(AuthService);
  private renderer = inject(Renderer2);
  @Inject(DOCUMENT) private document = inject(DOCUMENT);

  @ViewChild('profileMenu') private profileMenu !: Menu;
  private readonly STOP_SCROLLING_CLASS_NAME = 'stop-scrolling';
  protected profileMenuItems = computed(() => [
    {
      label: 'Settings',
      icon: 'fa-solid fa-sliders',
      route: ROUTES.ACCOUNTS,
      visible: this.authService.currentUser()?.roles.includes(AuthRole.ADMIN) ?? false,
    },
    {
      label: 'Profile',
      icon: 'fa-regular fa-user',
      route: ROUTES.PROFILE,
    },
    {
      label: 'Sign Out',
      icon: 'fa fa-right-to-bracket',
      command: () => {
        this.authService.logout();
      }
    }
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
