import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

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

  private readonly CREATE_INITIAL_COLLECTION = (name: string, collectionId: number) => {
    return {
      id: 0,
      name,
      parentId : collectionId,
      isMain: false,
    } as Collection;
  };

  private readonly CREATE_INITIAL_MARK = (name: string, collectionId: number) => {
    return {
      id : 0,
      name,
      collectionId,
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
    const { name, collectionId, type } = item;
    const collection : Collection = this.CREATE_INITIAL_COLLECTION(name, collectionId);
    return this.http.post<Collection>(`${ this.BASE_URL(type) }/create`, collection);
  }

  public createEmptyMark(item : CollectionItem): Observable<Mark> {
    const { name, collectionId, type } = item;
    const mark : Mark = this.CREATE_INITIAL_MARK(name, collectionId);
    return this.http.post<Mark>(`${ this.BASE_URL(type) }/create`, mark);
  }

  public rename( item: CollectionItem ): Observable<Collection | Mark> {
    const bodyRequest = {
        Id: item.typeId,
        name: item.name
    };

    return this.http.patch<Collection | Mark>(`${ this.BASE_URL(item.type) }/rename`, bodyRequest);
  }

  public updateFavoriteStatus(item : CollectionItem): Observable<boolean> {
    const bodyRequest = {
        Id : item.typeId,
        IsFavorite : !item.isFavorite,
    };
    return this.http.patch<boolean>(`${ this.BASE_URL(item.type) }/favorite`, bodyRequest);
  }

  public softDelete( item: CollectionItem ): Observable<boolean> {
    return this.http.delete<boolean>(`${ this.BASE_URL(item.type) }/softDelete/${ item.typeId }`);
  }
}