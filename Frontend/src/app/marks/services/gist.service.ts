import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { catchError, Observable, of } from 'rxjs';
import { GistResponse } from '../components/gist-manager/interfaces/gist';

@Injectable({providedIn: 'root'})
export class GistService {
    private baseUrl: string = `${ environment.baseApiUrl }/gists`;
    
    constructor( private http: HttpClient ) { }
    
    public getById( id: string ): Observable<GistResponse | null> {
        return this.http.get<GistResponse>(`${ this.baseUrl }/${ id }`)
        .pipe(catchError(() => of(null)));
    }
}