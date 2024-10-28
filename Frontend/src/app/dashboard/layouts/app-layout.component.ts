import { CommonModule } from '@angular/common';
import { Component, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { RouterModule } from '@angular/router';

import { MenuItem } from 'primeng/api';

import { SharedModule } from '../../shared/shared.module';
import { PrimengModule } from '../../shared/primeng/primeng.module';

@Component({
  selector: 'app-app-layout',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    SharedModule,
    PrimengModule
  ],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  templateUrl: './app-layout.component.html',
  styleUrl: './app-layout.component.css',
})
export class AppLayoutComponent {
  public menuItems: MenuItem[] = []
  public isBottomNavigationBarVisible : boolean = false;
  public width : number = window.innerWidth;

  public showBottomNavigationBar = ( menuItems: MenuItem[] ) => {
    this.width = window.innerWidth;
    this.menuItems = menuItems;
    this.isBottomNavigationBarVisible = true;
  }
}
