import { ROUTES } from '../../../../shared/utils/constant';
import { CommonModule } from '@angular/common';
import { Component, computed, inject, OnInit } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { CurrentRouteService } from '../../../../shared/services/current-route.service';
import { BreadCrumb } from '../../../../shared/interfaces/breadcrumb';

@Component({
  selector: 'app-bread-crumb',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule
  ],
  templateUrl: './bread-crumb.component.html',
  styleUrl: './bread-crumb.component.css',
})
export class BreadCrumbComponent implements OnInit {
  private router = inject(Router);
  private currentRouteService = inject(CurrentRouteService);

  public breadcrumbs = computed<BreadCrumb[]>(() => []);
  public homeUrl = ROUTES.HOME_URL;

  public ngOnInit(): void {
    this.breadcrumbs = this.currentRouteService.breadcrumbs;
  }

  public navigate(url : string) {
    this.router.navigate([url]);
  }
}
