import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, OnDestroy, OnInit, signal } from '@angular/core';
import { Observable, Subscription } from 'rxjs';

import { CollectionItemDataViewComponent } from '../../components/collection-item-data-view/collection-item-data-view.component';
import { CollectionItem } from '../../interfaces/collection-item';
import { CollectionItemPaginationService, CollectionItemTypeFilter } from '../../services/collection-item/pagination/collection-item-pagination.service';

@Component({
    selector: 'app-starred',
    imports: [
        CommonModule,
        CollectionItemDataViewComponent
    ],
    templateUrl: './starred.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class StarredComponent implements OnInit, OnDestroy {
  private itemsLoadingSubscription ?: Subscription;
  private itemsFilterSubscription ?: Subscription;
  public items$ : Observable<CollectionItem[]>;
  public currentFilter = signal<CollectionItemTypeFilter>(CollectionItemTypeFilter.All);
  public loading = signal<boolean>(false);

  get CollectionItemTypeFilter(): typeof CollectionItemTypeFilter {
    return CollectionItemTypeFilter;
  }

  constructor(private collectionItemPaginationService: CollectionItemPaginationService){
    this.items$ = collectionItemPaginationService.items$;
  }

  ngOnInit(): void {
    this.itemsLoadingSubscription = this.collectionItemPaginationService.loading$
      .subscribe((areItemsloading) => this.loading.set(areItemsloading));
    this.itemsFilterSubscription = this.collectionItemPaginationService.type$
      .subscribe((filter) => this.currentFilter.set(filter));
    this.resetAndFilterItems( CollectionItemTypeFilter.All );
  }

  ngOnDestroy(): void {
    if (this.itemsLoadingSubscription) this.itemsLoadingSubscription.unsubscribe();
    if (this.itemsFilterSubscription) this.itemsFilterSubscription.unsubscribe();
  }

  public onItemViewChange(itemIndex: number, itemsLength: number):void {
    if (itemIndex != itemsLength - 1) return;
    this.collectionItemPaginationService.loadNewPage();
  }

  public resetAndFilterItems( filter: CollectionItemTypeFilter ): void {
    this.collectionItemPaginationService.resetAndLoad({
      type: filter,
      collectionId : undefined,
      onlyFavorites : true
    });
  }
}
