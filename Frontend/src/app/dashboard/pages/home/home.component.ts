import { ChangeDetectionStrategy, Component, OnDestroy, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Observable } from 'rxjs';
import { Router, RouterModule } from '@angular/router';

import { DialogService, DynamicDialogConfig, DynamicDialogRef } from 'primeng/dynamicdialog';

import { CollectionItem, CollectionItemAction, CollectionItemType } from '../../../workplace/interfaces/collection-item';
import { CollectionItemDialogComponent } from '../../../workplace/components/collection-item-dialog/collection-item-dialog.component';
import { CollectionService } from '../../../workplace/services/collection.service';
import { DailyMarkActivityChartComponent } from '../../components/daily-mark-activity-chart/daily-mark-activity-chart.component';
import { DashboardReport } from '../../interfaces/dashboard-report';
import { DashboardService } from '../../services/dashboard.service';
import { ROUTES } from './../../../shared/utils/constant';
import { TimeAgoPipe } from '../../../shared/pipes/time-ago.pipe';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    DailyMarkActivityChartComponent,
    TimeAgoPipe,
  ],
  providers: [DialogService],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomeComponent implements OnInit, OnDestroy {
  private readonly DYNAMIC_DIALOG_CONFIG = (header: string, data: any): DynamicDialogConfig => {
    return {
      header,
      width  : '25rem',
      closable: false,
      dismissableMask: true,
      styleClass : 'custom-dialog',
      data : data
    }
  };
  private dialogReference: DynamicDialogRef | undefined;
  public mainCollectionId = signal<number | undefined>(undefined);
  public report$ : Observable<DashboardReport>;
  
  constructor(
    private collectionService: CollectionService,
    private dashboardService: DashboardService,
    private dialogService: DialogService,
    private router: Router,
  ) {
    this.report$ = this.dashboardService.getReportByCurrentSession();
  }
  
  public async ngOnInit(): Promise<void> {
    this.collectionService.getMainByCurrentSession()
    .subscribe(({ id }) => {
      this.mainCollectionId.set(id);
    });
  }

  public ngOnDestroy(): void {
    if (this.dialogReference) this.dialogReference.destroy();
  }

  public onNewCollectionBtnClick = () => this.addItem(CollectionItemType.Collection);
  public onNewMarkBtnClick = () => this.addItem(CollectionItemType.Mark);
  public onRecentMarkBtnClick = (markId: number) => this.router.navigate([ROUTES.MARKS_SEE(markId)]);
  public onStarredCollectionBtnClick = (collectionId: number) => this.router.navigate([ROUTES.COLLECTION_SEE(collectionId)]);
  public onViewAllRecentBtnClick = () => this.router.navigate([ROUTES.RECENT]);
  public onViewAllStarredBtnClick = () => this.router.navigate([ROUTES.STARRED]);

  private addItem( itemType: CollectionItemType ): void {
    const mainCollectionId = this.mainCollectionId();
    if (mainCollectionId === null || mainCollectionId === undefined)
      throw new Error("The mainCollectionId must be provided");
    
    const action = CollectionItemAction.Add;
    const header = `${ action } ${ itemType }`;
    const collectionItemEntry = {
      id: '',
      name : '',
      type : itemType,
      typeId : 0,
      collectionId: mainCollectionId,
    } as CollectionItem;
    const data = {
      action : action,
      collectionItem: collectionItemEntry,
    };

    // Open action dialog
    this.dialogReference = this.dialogService.open(
      CollectionItemDialogComponent,
      this.DYNAMIC_DIALOG_CONFIG(header, data)
    );
    
    // Subscribe to the onClose event of the dialog
    this.dialogReference.onClose.subscribe((returnedItemId: number) => {
      this.dialogReference = undefined;
      if (returnedItemId > 0) {
        const ROUTE_TO_FOLLOW = itemType === CollectionItemType.Collection
          ? ROUTES.COLLECTION_SEE(returnedItemId)
          : ROUTES.MARKS_SEE(returnedItemId);
        this.router.navigate([ROUTE_TO_FOLLOW]);
      }
    });
  }
}
