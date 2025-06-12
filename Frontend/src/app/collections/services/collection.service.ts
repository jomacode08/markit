import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { Collection } from '../interfaces/collection';

@Injectable({providedIn: 'root'})
export class CollectionService {
    private baseUrl: string = `${ environment.baseApiUrl }/collections`;

    constructor(private http: HttpClient) { }

    public getMainByCurrentSession(): Observable<Collection> {
        return this.http.get<Collection>(`${ this.baseUrl }/getMainByCurrentSession`);
    }

    public getById(collectionId: number): Observable<Collection> {
        return this.http.get<Collection>(`${ this.baseUrl }/getById/${ collectionId }`);
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