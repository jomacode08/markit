import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';
import { DashboardReport } from '../interfaces/dashboard-report';

@Injectable({providedIn: 'root'})
export class DashboardService {
    private baseUrl: string = `${ environment.baseApiUrl }/dashboard`;
    
    constructor(private http: HttpClient) { }

    public getReportByCurrentSession(): Observable<DashboardReport> {
        const clientTimeZone = Intl.DateTimeFormat().resolvedOptions().timeZone;
        return this.http.get<DashboardReport>(`${ this.baseUrl }`, {
            params: { 'clientTimeZone' : clientTimeZone }
        });
    }
}