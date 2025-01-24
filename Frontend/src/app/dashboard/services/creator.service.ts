import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { Creator } from './../interfaces/creator';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class CreatorService {

  private baseUrl: string = `${ environment.baseApiUrl }/creators`;

  constructor(private http: HttpClient) { }

  public getById(id: number): Observable<Creator> {
    return this.http.get<Creator>(`${ this.baseUrl }/getById/${ id }`);
  }

  public getByCurrentSession(): Observable<Creator> {
    return this.http.get<Creator>(`${ this.baseUrl }/getByCurrentSession`);
  }

  public update( creator: Creator ): Observable<Creator> {
    return this.http.put<Creator>(`${ this.baseUrl }/update`, creator);
  }
}
