import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, finalize } from 'rxjs';

import { CollectionItem } from '../../interfaces/collection-item';
import { environment } from '../../../../environments/environment';

export enum CollectionItemTypeFilter {
  All = 'All',
  Collection = 'Collection',
  Mark = 'Mark'
}

export interface CollectionItemPage {
  items : CollectionItem[],
  newCursor ?: string;
  hasNextPage : boolean;
}

export interface CollectionItemPageRequest {
  collectionId : number;
  pageSize: number;
  filter : CollectionItemTypeFilter;
  cursor ?: string;
}

@Injectable({providedIn: 'root'})
export class CollectionItemService {
  private readonly PAGE_SIZE = 20;
  private readonly BASE_URL: string = `${ environment.baseApiUrl }/collections`;
  private items = new BehaviorSubject<CollectionItem[]>([]);
  private filter = new BehaviorSubject<CollectionItemTypeFilter>(CollectionItemTypeFilter.All);
  private loading = new BehaviorSubject<boolean>(false);
  private hasNextPage : boolean = true;
  private isResetEnabled : boolean = false;
  private collectionId ?: number;
  private cursor ?: string;

  //! To the external world
  public items$ = this.items.asObservable();
  public filter$ = this.filter.asObservable();
  public loading$ = this.loading.asObservable();

  constructor(private http: HttpClient) { }

  public loadNewPage(): void {
    if (this.loading.value) return;
    if (!this.hasNextPage) return;
    if (!this.collectionId) return;
    
    this.loading.next(true);

    let bodyRequest = {
      collectionId : this.collectionId,
      pageSize: this.PAGE_SIZE,
      filter: this.filter.value,
      cursor: this.cursor
    } as CollectionItemPageRequest;

    this.http.post<CollectionItemPage>(`${ this.BASE_URL }/getChildrenPaged`, bodyRequest)
    .pipe(
      finalize(() => this.loading.next(false))
    )
    .subscribe((page) => {
      const currentItems = this.isResetEnabled ? [] : this.items.value;
      this.items.next([...currentItems, ...page.items ?? []]);
      this.cursor = page.newCursor;
      this.hasNextPage = page.hasNextPage;
      this.isResetEnabled = this.isResetEnabled ? false : this.isResetEnabled;
    });
  }

  public resetAndLoad( collectionId: number, filter: CollectionItemTypeFilter ): void {
    this.filter.next(filter);
    this.isResetEnabled = true;
    this.hasNextPage = true;
    this.cursor = undefined;
    this.collectionId = collectionId;
    this.loadNewPage();
  }

  public updateItem(updatedItem: CollectionItem, index: number): void {
    const items = this.items.getValue();
    items[index] = updatedItem;
    this.items.next(items);
  }
}