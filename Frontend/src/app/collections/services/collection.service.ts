import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';
import { CollectionItem } from '../interfaces/collection-item';
import { Collection } from '../interfaces/collection';

@Injectable({providedIn: 'root'})
export class CollectionService {
    private baseUrl: string = `${ environment.baseApiUrl }/collections`;

    constructor(private http: HttpClient) { }

    public getCollectionById(collectionId: number): Observable<Collection> {
        return this.http.get<Collection>(`${ this.baseUrl }/getCollectionById/${ collectionId }`);
    }

    public getRootCollectionsForGrid(): Observable<CollectionItem[]> {
        return this.http.get<CollectionItem[]>(`${ this.baseUrl }/getRootCollectionsForGrid`);
    }

    public create(collection: Collection): Observable<Collection> {
        return this.http.post<Collection>(`${ this.baseUrl }/create`, collection);
    }

    public rename( id: number, newName: string ): Observable<Collection> {
        const bodyRequest = {
            Id: id,
            name: newName
        };
        return this.http.patch<Collection>(`${ this.baseUrl }/rename`, bodyRequest);
    }

    public softDelete( id:number ): Observable<boolean> {
        return this.http.delete<boolean>(`${ this.baseUrl }/softDelete/${ id }`);
    }
}