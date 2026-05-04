import { ChangeDetectionStrategy, Component } from '@angular/core';
import { MenuItem } from 'primeng/api';
import { MenuModule } from "primeng/menu";
import { ROUTES } from '../../shared/utils/constant';
import { RouterModule } from '@angular/router';

@Component({
    selector: 'app-settings-layout',
    imports: [MenuModule, RouterModule],
    templateUrl: './settings-layout.component.html',
    styleUrl: './settings-layout.component.css',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class SettingsLayoutComponent {
    protected settingItems : MenuItem[] = [
      {
        label: 'Accounts',
        icon: 'fa-solid fa-users',
        routerLink: ROUTES.ACCOUNTS,
      },
      {
        label: 'Authentication',
        icon: 'fa-solid fa-lock',
        routerLink: ROUTES.AUTHENTICATION_SETTINGS,
      },
      {
        label: 'Demo',
        icon: 'fa-solid fa-flask',
        routerLink: ROUTES.DEMO_FORM,
      },
    ];
}
