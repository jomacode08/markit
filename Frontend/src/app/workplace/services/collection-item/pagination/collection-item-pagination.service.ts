import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, finalize } from 'rxjs';

import { CollectionItem } from '../../../interfaces/collection-item';
import { environment } from '../../../../../environments/environment';

export interface CollectionItemPageRequest {
  pageSize : number,
  cursor ?: string,
  sortOrder : SortPaginationOrder,
  filters : CollectionItemFilters
}

export interface CollectionItemPage {
  items : CollectionItem[],
  newCursor ?: string;
  hasNextPage : boolean;
}

export interface CollectionItemFilters {
  type: CollectionItemTypeFilter,
  collectionId ?: number,
  onlyFavorites : boolean,
}

export enum CollectionItemTypeFilter {
  All = 'All',
  Collection = 'Collection',
  Mark = 'Mark'
}

export enum SortPaginationOrder {
  Ascending = 'Ascending',
  Descending = 'Descending',
}

@Injectable({providedIn: 'root'})
export class CollectionItemPaginationService {
  private readonly BASE_URL: string = `${ environment.baseApiUrl }/collections`;
  private readonly PAGE_SIZE = 20;

  private cursor ?: string;
  private hasNextPage : boolean = true;
  private isResetEnabled : boolean = false;
  private sortOrder : SortPaginationOrder = SortPaginationOrder.Ascending;

  private itemsSubject = new BehaviorSubject<CollectionItem[]>([]);
  private typeSubject = new BehaviorSubject<CollectionItemTypeFilter>(CollectionItemTypeFilter.All);
  private loadingSubject = new BehaviorSubject<boolean>(false);
  
  private filters : CollectionItemFilters = {
    type : this.typeSubject.value,
    collectionId : undefined,
    onlyFavorites : false
  };
  
  //! To the external world
  public items$ = this.itemsSubject.asObservable();
  public type$ = this.typeSubject.asObservable();
  public loading$ = this.loadingSubject.asObservable();

  constructor(
    private http: HttpClient
  ) { }

  public loadNewPage(): void {
    if (this.loadingSubject.value) return;
    if (!this.hasNextPage) return;
    
    this.loadingSubject.next(true);

    let bodyRequest = {
      pageSize : this.PAGE_SIZE,
      cursor : this.cursor,
      sortOrder : this.sortOrder,
      filters : this.filters
    } as CollectionItemPageRequest;

    this.http.post<CollectionItemPage>(`${ this.BASE_URL }/search`, bodyRequest)
    .pipe(
      finalize(() => this.loadingSubject.next(false))
    )
    .subscribe((page) => {
      const currentItems = this.isResetEnabled ? [] : this.itemsSubject.value;
      this.itemsSubject.next([...currentItems, ...page.items]);
      this.cursor = page.newCursor;
      this.hasNextPage = page.hasNextPage;
      this.isResetEnabled = this.isResetEnabled ? false : this.isResetEnabled;
    });
  }

  public resetAndLoad(
    filters : CollectionItemFilters,
    sortOrder : SortPaginationOrder = SortPaginationOrder.Ascending
  ): void {
    this.filters = filters;
    this.sortOrder = sortOrder;
    this.isResetEnabled = true;
    this.cursor = undefined;
    this.hasNextPage = true;
    this.typeSubject.next(filters.type);
    this.loadNewPage();
  }
}