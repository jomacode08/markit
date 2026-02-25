import { ChangeDetectionStrategy, Component } from '@angular/core';
import { MenuItem } from 'primeng/api';
import { MenuModule } from "primeng/menu";
import { ROUTES } from '../../shared/utils/constant';

@Component({
  selector: 'app-settings-layout',
  standalone: true,
  imports: [MenuModule],
  templateUrl: './settings-layout.component.html',
  styleUrl: './settings-layout.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SettingsLayoutComponent {
    protected settingItems : MenuItem[] = [
      {
        label: 'Accounts',
        icon: 'fa-solid fa-users',
        routerLink: ROUTES.ACCOUNTS,
      }
    ];
}
