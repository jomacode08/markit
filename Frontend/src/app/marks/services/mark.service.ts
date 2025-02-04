import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';
import { Mark } from '../interfaces/mark';

@Injectable({providedIn: 'root'})
export class MarkService {
    private baseUrl: string = `${ environment.baseApiUrl }/marks`;

    constructor(private http: HttpClient) { }
    
    public getById(id: number): Observable<Mark> {
        return this.http.get<Mark>(`${ this.baseUrl }/getById/${ id }`);
    }

    public getByCurrentSession(): Observable<Mark[]> {
        return this.http.get<Mark[]>(`${ this.baseUrl }/getByCurrentSession`);
    }

    public create(mark: Mark): Observable<Mark> {
        return this.http.post<Mark>(`${ this.baseUrl }/create`, mark);
    }

    public patch(mark : Mark): Observable<Mark> {
        return this.http.patch<Mark>(`${ this.baseUrl }/update`, mark);
    }

    public delete(id: number): Observable<boolean> {
        return this.http.delete<boolean>(`${ this.baseUrl }/delete/${ id }`);
    }
}