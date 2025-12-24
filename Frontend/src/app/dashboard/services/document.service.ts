import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

import { environment } from '../../../environments/environment';
import { DocumentSearch } from '../interfaces/search/document-search';

export enum SearchFilters {
    All = 'All',
    Collections = 'Collections',
    Marks = 'Marks'
}

@Injectable({ providedIn: 'root' })
export class DocumentService {
    private baseUrl: string = `${ environment.baseApiUrl }/documents`;

    constructor(
        private http: HttpClient,
    ) {}

    public search(query: string, filter: SearchFilters): Observable<DocumentSearch> {
        return this.http.post<DocumentSearch>(`${ this.baseUrl }/search`,{
            query,
            filter,
        });
    }
}