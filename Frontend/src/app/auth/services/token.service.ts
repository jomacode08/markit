import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';

@Injectable({providedIn: 'root'})
export class TokenService {
    private baseUrl: string = `${environment.baseApiUrl}/token`;
    
    constructor(
        private http: HttpClient
    ) {}

    public refresh(): Observable<void> {
        return this.http.post<void>(`${ this.baseUrl }/refresh`, {})
    }
}