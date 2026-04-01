import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { Collection } from '../interfaces/collection';
import { CollectionNode } from '../interfaces/collection-node';

@Injectable({providedIn: 'root'})
export class CollectionService {
    private baseUrl: string = `${ environment.baseApiUrl }/collections`;

    constructor(private http: HttpClient) { }

    public getMainByCurrentSession(): Observable<Collection> {
        return this.http.get<Collection>(`${ this.baseUrl }/main`);
    }

    public getById(collectionId: number): Observable<Collection> {
        return this.http.get<Collection>(`${ this.baseUrl }/${ collectionId }`);
    }

    public getTree(): Observable<CollectionNode> {
        return this.http.get<CollectionNode>(`${ this.baseUrl }/tree`);
    }
}