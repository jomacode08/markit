import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { Block } from '../interfaces/block';
import { environment } from './../../../environments/environment';

@Injectable({providedIn: 'root'})
export class BlockService {
    private baseUrl: string = `${ environment.baseApiUrl }/blocks`;
    
    constructor( private http: HttpClient ) { }

    public updateContent( id: number, content: string ): Observable<Block> {
        const bodyRequest = { id, content };
        return this.http.patch<Block>(`${ this.baseUrl }/update-content`, bodyRequest);
    }
}