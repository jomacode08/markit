import { Component, EventEmitter, Output } from '@angular/core';
import { MenuItem } from 'primeng/api';

@Component({
  selector: 'shared-main-bar',
  templateUrl: './main-bar.component.html',
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
      icon: 'fa fa-right-to-bracket'
    },
  ];

  @Output()
  public onNavigationAction = new EventEmitter<MenuItem[]>();
}
