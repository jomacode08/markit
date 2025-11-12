import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { catchError, Observable, of, shareReplay, tap } from 'rxjs';
import { GistResponse } from '../components/gist-manager/interfaces/gist';

@Injectable({providedIn: 'root'})
export class GistService {
    private baseUrl: string = `${ environment.baseApiUrl }/gists`;
    private cache = new Map<string, GistResponse>();
    
    constructor( private http: HttpClient ) { }
    
    public getById( id: string ): Observable<GistResponse | null> {
        const cacheRequested = this.cache.get(id);
        if (cacheRequested) return of(cacheRequested);

        return this.http.get<GistResponse>(`${ this.baseUrl }/${ id }`)
        .pipe(
            tap(response => this.cache.set(id, response)),
            shareReplay(1),
            catchError(() => of(null),
        ));
    }

    public clearCacheItem = ( id:string ) => this.cache.delete(id);
}