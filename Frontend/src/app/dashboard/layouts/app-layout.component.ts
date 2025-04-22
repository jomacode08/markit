import { CommonModule } from '@angular/common';
import { Component, computed, OnInit, ViewChild } from '@angular/core';
import { RouterModule } from '@angular/router';

import { MenuItem } from 'primeng/api';
import { MenuModule } from 'primeng/menu';
import { SidebarModule, Sidebar } from 'primeng/sidebar';

import { MainBarComponent } from './components/main-bar/main-bar.component';
import { AuthService } from '../../auth/services/auth.service';
import { CurrentRouteService } from '../../shared/services/current-route.service';
import { GeneralConstant } from '../../shared/utils/general-constant';

@Component({
  selector: 'app-app-layout',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MainBarComponent,
    MenuModule,
    SidebarModule
  ],
  templateUrl: './app-layout.component.html',
  styleUrl: './app-layout.component.css',
})
export class AppLayoutComponent implements OnInit {
  //* Route Configuration
  public titleComponent  = computed<string>(() => '');
  public url = computed<string>(() => '');
  public homeUrl: string = GeneralConstant.HOME_URL; 

  //* Menu Congiguration
  @ViewChild('sideBar')
  public sideBarRef !: Sidebar;
  public menuItems: MenuItem[] = []
  public isSideBarVisible : boolean = false;
  public width : number = window.innerWidth;
  public userName ?: string;
  
  constructor(
    private authService: AuthService,
    private currentRouteService : CurrentRouteService
  ) {}
  
  ngOnInit(): void {
    this.userName = this.authService.currentUser()?.given_name;
    this.titleComponent = this.currentRouteService.title;
    this.url = this.currentRouteService.url;
  }

  public showSideBar = ( menuItems: MenuItem[] ) => {
    this.width = window.innerWidth;
    this.menuItems = menuItems;
    this.isSideBarVisible = true;
  }

  public hideSideBar = () => {
    this.sideBarRef.close(new Event('click'));
  }
}
