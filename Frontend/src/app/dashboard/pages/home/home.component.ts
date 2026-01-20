import { ChangeDetectionStrategy, Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Observable } from 'rxjs';
import { Router, RouterModule } from '@angular/router';

import { DialogService } from 'primeng/dynamicdialog';

import { DailyMarkActivityChartComponent } from '../../components/daily-mark-activity-chart/daily-mark-activity-chart.component';
import { DashboardReport } from '../../interfaces/dashboard-report';
import { DashboardService } from '../../services/dashboard.service';
import { ROUTES } from './../../../shared/utils/constant';
import { TimeAgoPipe } from '../../../shared/pipes/time-ago.pipe';
import { HeroComponent } from './components/hero/hero.component';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [
    CommonModule,
    DailyMarkActivityChartComponent,
    HeroComponent,
    RouterModule,
    TimeAgoPipe
  ],
  providers: [DialogService],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomeComponent {
  public report$ : Observable<DashboardReport>;
  
  constructor(
    private dashboardService: DashboardService,
    private router: Router,
  ) {
    this.report$ = this.dashboardService.getReportByCurrentSession();
  }

  public onRecentMarkBtnClick = (markId: number) => this.router.navigate([ROUTES.MARKS_SEE(markId)]);
  public onStarredCollectionBtnClick = (collectionId: number) => this.router.navigate([ROUTES.COLLECTION_SEE(collectionId)]);
  public onViewAllRecentBtnClick = () => this.router.navigate([ROUTES.RECENT]);
  public onViewAllStarredBtnClick = () => this.router.navigate([ROUTES.STARRED]);

  public isLastRow(index: number, cols: number, itemsLength: number): boolean {
    const currentRow = Math.floor(index / cols); 
    const lastRow = Math.floor((itemsLength - 1) / cols);
    return currentRow === lastRow;
  }
}
