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
export class SearchService {
    private baseUrl: string = `${ environment.baseApiUrl }/search`;

    constructor(
        private http: HttpClient,
    ) {}

    public searchDocuments(query: string, filter: SearchFilters): Observable<DocumentSearch> {
        return this.http.post<DocumentSearch>(`${ this.baseUrl }/documents`,{
            query,
            filter,
        });
    }
}