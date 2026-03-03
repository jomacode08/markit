import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { DemoSettings } from '../interfaces/demo-settings';
import { environment } from '../../../environments/environment';
import { DemoStatus } from '../interfaces/demo-status';

@Injectable({providedIn: 'root'})
export class DemoService {
    private readonly BASE_URL: string = `${ environment.baseApiUrl }/auth/demo`;

    constructor(private http: HttpClient) {}

    public getStatus(): Observable<DemoStatus> {
        return this.http.get<DemoStatus>(`${ this.BASE_URL }/status`);
    }

    public getSettings(): Observable<DemoSettings> {
        return this.http.get<DemoSettings>(`${ this.BASE_URL }/settings`);
    }

    public updateSettings(settings : DemoSettings): Observable<DemoSettings> {
        return this.http.put<DemoSettings>(`${ this.BASE_URL }/settings`, settings);
    }
}