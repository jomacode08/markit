import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map, Observable } from 'rxjs';

import { CollectionItem, CollectionItemType } from '../../../interfaces/collection-item';
import { environment } from '../../../../../environments/environment';
import { Collection } from '../../../interfaces/collection';
import { Mark } from '../../../../marks/interfaces/mark';
import { DEFAULT_BLOCK_NAME } from '../../../../shared/utils/constant';

@Injectable({providedIn: 'root'})
export class CollectionItemActionService {
  private readonly BASE_URL = (type: CollectionItemType) => {  
    return `${ environment.baseApiUrl }/${ type === CollectionItemType.Collection ? 'collections' : 'marks' }`;
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

  private readonly CREATE_INITIAL_MARK = (name: string, collectionId: number, emoji ?: string) => {
    return {
      id : 0,
      name,
      collectionId,
      emoji,
      creatorId : 0,
      blocks: [
        {
          id : 0,
          title : DEFAULT_BLOCK_NAME,
          content: ''
        }
      ],
    } as Mark;
  };

  constructor(private http: HttpClient) {}

  public createEmptyCollection(item : CollectionItem): Observable<Collection> {
    const { name, collectionId, type, emoji } = item;
    const collection : Collection = this.CREATE_INITIAL_COLLECTION(name, collectionId, emoji);
    return this.http.post<Collection>(`${ this.BASE_URL(type) }`, collection);
  }

  public createEmptyMark(item : CollectionItem): Observable<Mark> {
    const { name, collectionId, type, emoji } = item;
    const mark : Mark = this.CREATE_INITIAL_MARK(name, collectionId, emoji);
    return this.http.post<Mark>(`${ this.BASE_URL(type) }`, mark);
  }

  public rename( item: CollectionItem ): Observable<Collection | Mark> {
    const bodyRequest = {
      name: item.name,
      emoji : item.emoji
    };

    return this.http.patch<Collection | Mark>(`${ this.BASE_URL(item.type) }/${ item.typeId }`, bodyRequest);
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