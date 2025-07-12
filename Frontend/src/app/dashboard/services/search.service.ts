import { catchError, map, Observable, of } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

import { AuthService } from '../../auth/services/auth.service';
import { CollectionDocument } from '../interfaces/search/documents/collection-document';
import { CustomMessageService } from '../../shared/services/custom-message.service';
import { MarkDocument } from '../interfaces/search/documents/mark-document';
import { MEILISEARCH } from '../../shared/utils/constant';
import { meiliSearchEnvironment } from '../../../environments/environment';
import { MultiSearchRequest, MultiSearchResponse, Query } from '../interfaces/search/multi-search';
import { SearchResults } from '../interfaces/search/search-results';

export enum SearchFilters {
    All = 'All',
    Collections = 'Collections',
    Marks = 'Marks'
}

@Injectable({ providedIn: 'root' })
export class SearchService {
    private readonly SEARCH_LIMIT: number = 10;
    private readonly generalError = "Something went wrong, we keep track of this error, but feel free to contact us if refreshing doesn't fix things.";

    constructor(
        private http: HttpClient,
        private authService: AuthService,
        private messageService: CustomMessageService
    ) { }

    public search(query: string, filter: SearchFilters): Observable<SearchResults | null> {
        const requestUrl = `${meiliSearchEnvironment.serverUrl}/multi-search`;
        const body = this.constructSearchBodyRequest(query, filter);

        return this.http.post<MultiSearchResponse>(requestUrl, body, {
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${this.authService.meiliSearchToken()}`
            }
        }).pipe(
            map(result => this.mapSearchResults(result)),
            catchError(err => {
                this.messageService.showGeneralError(this.generalError);
                return of(null);
            })
        );
    }

    private constructSearchBodyRequest(query: string, filter: SearchFilters): MultiSearchRequest {
        let queries: Query[] = [];

        switch (filter) {
            case SearchFilters.All: {
                queries.push(this.createNewQuery(
                    query,
                    MEILISEARCH.COLLECTION_INDEX_UID,
                    MEILISEARCH.COLLECTION_SEARCHABLE_ATTRIBUTE_NAME
                ));
                queries.push(this.createNewQuery(
                    query,
                    MEILISEARCH.MARK_INDEX_UID,
                    MEILISEARCH.MARK_SEARCHABLE_ATTRIBUTE_NAME
                ));
                break;
            }
            case SearchFilters.Collections: {
                queries.push(this.createNewQuery(
                    query,
                    MEILISEARCH.COLLECTION_INDEX_UID,
                    MEILISEARCH.COLLECTION_SEARCHABLE_ATTRIBUTE_NAME
                ));
                break;
            }
            case SearchFilters.Marks: {
                queries.push(this.createNewQuery(
                    query,
                    MEILISEARCH.MARK_INDEX_UID,
                    MEILISEARCH.MARK_SEARCHABLE_ATTRIBUTE_NAME
                ));
                break;
            }
            default:
                break;
        }

        return {
            queries
        } as MultiSearchRequest;
    }

    private createNewQuery
    (
        query: string,
        indexUid: string,
        searchableAttribute: string,
    ): Query {
        return {
            indexUid,
            q: query,
            limit: this.SEARCH_LIMIT,
            attributesToHighlight: [searchableAttribute],
            highlightPreTag: "<span>",
            highlightPostTag: "</span>"
        } as Query;
    }

    private mapSearchResults(data: MultiSearchResponse): SearchResults {
        const results = data.results;
        const collectionIndexResult = results.find(r => r.indexUid === MEILISEARCH.COLLECTION_INDEX_UID);
        const markIndexResult = results.find(r => r.indexUid === MEILISEARCH.MARK_INDEX_UID);

        return {
            collectionDocuments: collectionIndexResult?.hits.map(h => h._formatted as CollectionDocument) ?? [],
            markDocuments: markIndexResult?.hits.map(h => h._formatted as MarkDocument) ?? [],
        };
    }
}