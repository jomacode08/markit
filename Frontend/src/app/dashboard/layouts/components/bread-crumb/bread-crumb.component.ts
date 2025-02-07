import { CommonModule } from '@angular/common';
import { Component, computed, inject, OnInit } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { CurrentRouteService } from '../../../../shared/services/current-route.service';
import { BreadCrumb } from '../../../../shared/interfaces/breadcrumb';
import { GeneralConstant } from '../../../../shared/utils/general-constant';

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
  public homeUrl = GeneralConstant.HOME_URL;

  public ngOnInit(): void {
    this.breadcrumbs = this.currentRouteService.breadcrumbs;
  }

  public navigate(url : string) {
    this.router.navigate([url]);
  }
}
