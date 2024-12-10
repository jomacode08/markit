import { CommonModule } from '@angular/common';
import { Component, CUSTOM_ELEMENTS_SCHEMA, ViewChild } from '@angular/core';
import { ActivatedRoute, NavigationEnd, Router, RouterModule } from '@angular/router';

import { MenuItem } from 'primeng/api';

import { PrimengModule } from '../../shared/primeng/primeng.module';
import { MainBarComponent } from './components/main-bar/main-bar.component';
import { BreadCrumbComponent } from "./components/bread-crumb/bread-crumb.component";
import { Sidebar } from 'primeng/sidebar';
import { AuthService } from '../../auth/services/auth.service';

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
  @ViewChild('sideBar')
  public sideBarRef !: Sidebar;
  public menuItems: MenuItem[] = []
  public isSideBarVisible : boolean = false;
  public width : number = window.innerWidth;
  public userName ?: string;
  
  constructor(
    private authService: AuthService,
    private router: Router,
    private activatedRoute: ActivatedRoute,
  ) {
    this.handleRouterEvents();
    this.userName = this.authService.currentUser()?.given_name;
  }

  public showSideBar = ( menuItems: MenuItem[] ) => {
    this.width = window.innerWidth;
    this.menuItems = menuItems;
    this.isSideBarVisible = true;
  }

  public hideSideBar = () => {
    this.sideBarRef.close(new Event('click'));
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
