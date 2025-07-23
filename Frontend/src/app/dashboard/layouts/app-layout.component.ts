import { CommonModule } from '@angular/common';
import { Component, Signal, signal } from '@angular/core';
import { RouterModule } from '@angular/router';

import { MenuItem } from 'primeng/api';
import { SidebarModule } from 'primeng/sidebar';

import { MainBarComponent } from './components/main-bar/main-bar.component';
import { CurrentRouteService } from '../../shared/services/current-route.service';
import { ROUTES } from '../../shared/utils/constant';
import { QuickSearchComponent } from '../components/quick-search/quick-search.component';
import { SidebarConfig, SidebarContentType } from '../interfaces/sidebar-config';
import { NavigationMenuComponent } from './components/navigation-menu/navigation-menu.component';

@Component({
  selector: 'app-app-layout',
  standalone: true,
  imports: [
    CommonModule,
    MainBarComponent,
    QuickSearchComponent,
    RouterModule,
    SidebarModule,
    NavigationMenuComponent,
  ],
  templateUrl: './app-layout.component.html',
  styleUrl: './app-layout.component.css'
})
export class AppLayoutComponent {
  //* General configuration
  public width : number = window.innerWidth;
  public homeUrl: string = ROUTES.HOME_URL;
  //* Route
  public titleComponent: Signal<string>;
  public url : Signal<string>;
  //* Sidebar
  public isSidebarVisible = signal(false);
  public sideBarConfig = signal<SidebarConfig | undefined>(undefined);
  
  constructor( private currentRouteService : CurrentRouteService) {
    this.titleComponent = this.currentRouteService.title;
    this.url = this.currentRouteService.url;
  }

  public showSidebar = (content: SidebarContentType, navigationItems ?: MenuItem[]) => {
    this.width = window.innerWidth;
    this.sideBarConfig.set({
      contentType : content,
      title : content === 'Searching' ? 'Search' : 'Menu',
      navigationItems: navigationItems ?? []
    } as SidebarConfig);
    this.toggleSidebar();
  }

  public toggleSidebar = () => this.isSidebarVisible.set(!this.isSidebarVisible()); 
}
