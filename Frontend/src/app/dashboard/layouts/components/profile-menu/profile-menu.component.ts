import { ChangeDetectionStrategy, Component, Inject, Renderer2, ViewChild } from '@angular/core';
import { DOCUMENT, NgIf } from '@angular/common';

import { MenuItem } from 'primeng/api';
import { Menu, MenuModule } from 'primeng/menu';

import { ROUTES } from '../../../../shared/utils/constant';
import { AuthService } from '../../../../auth/services/auth.service';

@Component({
  selector: 'app-profile-menu',
  standalone: true,
  imports: [
    MenuModule,
    NgIf
  ],
  template: `
    <p-menu
      #profileMenu
      styleClass="popup"
      appendTo="body"
      [model]="profileMenuItems"
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
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProfileMenu {
  @ViewChild('profileMenu') private profileMenu !: Menu;
  private readonly STOP_SCROLLING_CLASS_NAME = 'stop-scrolling';
  protected profileMenuItems: MenuItem[] = [
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
  ];

  constructor(
    private authService: AuthService,
    private renderer: Renderer2,
    @Inject(DOCUMENT) private document: Document
  ) {}

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
