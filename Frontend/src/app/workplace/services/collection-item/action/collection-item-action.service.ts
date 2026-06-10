import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map, Observable } from 'rxjs';

import { CollectionItem, CollectionItemType } from '../../../interfaces/collection-item';
import { environment } from '../../../../../environments/environment';
import { Collection } from '../../../interfaces/collection';
import { Notebook } from '../../../../notebooks/interfaces/notebook';
import { DEFAULT_BLOCK_NAME } from '../../../../shared/utils/constant';

@Injectable({providedIn: 'root'})
export class CollectionItemActionService {
  private readonly BASE_URL = (type: CollectionItemType) => {  
    return `${ environment.baseApiUrl }/${ type === CollectionItemType.Collection ? 'collections' : 'notebooks' }`;
  };

  private readonly CREATE_INITIAL_COLLECTION = (name: string, collectionId: number, emoji ?: string) => {
    return {
      id: 0,
      name,
      emoji,
      parentId : collectionId,
      isMain: false,
    } as Collection;
  };

  private readonly CREATE_INITIAL_NOTEBOOK = (name: string, collectionId: number, emoji ?: string) => {
    return {
      id : 0,
      name,
      collectionId,
      emoji,
      userId : '',
      blocks: [
        {
          id : 0,
          title : DEFAULT_BLOCK_NAME,
          content: ''
        }
      ],
    } as Notebook;
  };

  constructor(private http: HttpClient) {}

  public createEmptyCollection(item : CollectionItem): Observable<Collection> {
    const { name, collectionId, type, emoji } = item;
    const collection : Collection = this.CREATE_INITIAL_COLLECTION(name, collectionId, emoji);
    return this.http.post<Collection>(`${ this.BASE_URL(type) }`, collection);
  }

  public createEmptyNotebook(item : CollectionItem): Observable<Notebook> {
    const { name, collectionId, type, emoji } = item;
    const notebook : Notebook = this.CREATE_INITIAL_NOTEBOOK(name, collectionId, emoji);
    return this.http.post<Notebook>(`${ this.BASE_URL(type) }`, notebook);
  }

  public rename( item: CollectionItem ): Observable<Collection | Notebook> {
    const bodyRequest = {
      name: item.name,
      emoji : item.emoji
    };

    return this.http.patch<Collection | Notebook>(`${ this.BASE_URL(item.type) }/${ item.typeId }`, bodyRequest);
  }

  public updateFavoriteStatus(item : CollectionItem): Observable<boolean> {
    const URL = `${ this.BASE_URL(item.type) }/${item.typeId}/favorite`;
    if (item.isFavorite) {
      return this.http.delete<boolean>(URL)
      .pipe(map(() => !item.isFavorite));
    } else {
      return this.http.put<boolean>(URL, {})
      .pipe(map(() => !item.isFavorite));
    }
  }

  public softDelete( item: CollectionItem ): Observable<boolean> {
    return this.http.delete<boolean>(`${ this.BASE_URL(item.type) }/${ item.typeId }`);
  }

  public move( itemToMove: CollectionItem, collectionId: number ): Observable<void> {
    const body = { parentId: collectionId };
    const URL = `${ this.BASE_URL(itemToMove.type) }/${itemToMove.typeId}/move`;
    return this.http.post<void>(URL, body);
  }
}