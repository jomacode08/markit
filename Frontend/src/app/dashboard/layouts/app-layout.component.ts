import { CommonModule } from '@angular/common';
import { Component, CUSTOM_ELEMENTS_SCHEMA, OnInit, signal } from '@angular/core';
import { ActivatedRoute, NavigationEnd, Router, RouterModule } from '@angular/router';

import { MenuItem } from 'primeng/api';

import { PrimengModule } from '../../shared/primeng/primeng.module';
import { MainBarComponent } from './components/main-bar/main-bar.component';
import { BreadCrumbComponent } from "./components/bread-crumb/bread-crumb.component";
import { Observable } from 'rxjs';

@Component({
  selector: 'app-app-layout',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    PrimengModule,
    MainBarComponent,
    BreadCrumbComponent
],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  templateUrl: './app-layout.component.html',
  styleUrl: './app-layout.component.css',
})
export class AppLayoutComponent {
  //* Route Configuration
  public titleComponent ?: string;
  public activatedUrl   ?: string;

  //* Menu Congifuration
  public menuItems: MenuItem[] = []
  public isBottomNavigationBarVisible : boolean = false;
  public width : number = window.innerWidth;
  
  public showBottomNavigationBar = ( menuItems: MenuItem[] ) => {
    this.width = window.innerWidth;
    this.menuItems = menuItems;
    this.isBottomNavigationBarVisible = true;
  }

  constructor(private router: Router, private activatedRoute: ActivatedRoute) {
    this.handleRouterEvents();
  }

  private handleRouterEvents(): void {
    this.router.events.subscribe((event) => {
      if (event instanceof NavigationEnd) {
        this.activatedUrl = this.router.url;
        this.titleComponent = this.activatedRoute.firstChild?.routeConfig?.title?.toString();
      }
    });
  }
}
