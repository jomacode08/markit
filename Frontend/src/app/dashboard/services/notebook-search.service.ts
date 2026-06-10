import { computed, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { catchError, Observable, of, tap } from 'rxjs';

import { environment } from '../../../environments/environment';
import { NotebookSearchResult } from '../interfaces/search/notebook-search-result';

export enum SearchState {
  idle,
  searching,
  finished,
  error
};

@Injectable({providedIn: 'root'})
export class NotebookSearchService {
    private readonly baseUrl: string = `${ environment.baseApiUrl }/notebooks/search`;
    private _results = signal<NotebookSearchResult[]>([]);
    private _searchTerm = signal<string>("");
    private _searchState = signal<SearchState>(SearchState.idle);
    
    //! To the external world
    public results = computed<NotebookSearchResult[]>(() => this._results());
    public searchTerm = computed<string>(() => this._searchTerm());
    public searchState = computed<SearchState>(() => this._searchState());

    constructor(private http: HttpClient) { }
    
    public search(searchTerm: string): Observable<NotebookSearchResult[] | null> {
        if (!this.isQueryValid(searchTerm)) {
            this.resetState();
            return of(null);
        }
        this._searchTerm.set(searchTerm);
        this._searchState.set(SearchState.searching);
        return this.http.get<NotebookSearchResult[]>(`${ this.baseUrl }`, {
            params : {searchTerm}
        }).pipe(
            catchError((error) => {
                this._results.set([]);
                this._searchState.set(SearchState.error);
                return of(null);
            }),
            tap((notebooks) => {
                if (notebooks) {
                    this._results.set(notebooks);
                    this._searchState.set(SearchState.finished);
                }
            })
        );
    }

    private isQueryValid(query: string): boolean {
        return query.trim().length > 0;
    }

    private resetState(): void {
        this._searchTerm.set("");
        this._searchState.set(SearchState.idle);
        this._results.set([]);
    }

}