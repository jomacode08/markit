import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { LoginProvider, LoginPurpose } from '../interfaces/signin-methods';
import { map, Observable, of } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ExternalAuthSettings } from '../../settings/interfaces/external-auth-settings';

interface LinkTokenResponse {
    token: string;
}

@Injectable({providedIn: 'root'})
export class ExternalLoginService {
    private baseUrl: string = `${environment.baseApiUrl}/auth/external`;
    
    constructor(private http: HttpClient) { }

    public remove(provider: LoginProvider): Observable<void> {
        return this.http.delete<void>(`${ this.baseUrl }/${ provider }`);
    }

    public getSettings(): Observable<ExternalAuthSettings> {
        return this.http.get<ExternalAuthSettings>(`${ this.baseUrl }/settings`);
    }

    public updateSettings(settings: ExternalAuthSettings): Observable<ExternalAuthSettings> {
        return this.http.put<ExternalAuthSettings>(`${ this.baseUrl }/settings`, settings);
    }

    public getLoginUrlForProvider(
        provider: LoginProvider,
        purpose: LoginPurpose
    ): Observable<string> {
        const baseUrl = `${this.baseUrl}/${provider}`;

        if (purpose === LoginPurpose.LinkAccount) {
            return this.getLinkToken()
            .pipe(map(token => `${baseUrl}/account/${token}`));
        }

        return of(baseUrl);
    }

    private getLinkToken(): Observable<string> {
        return this.http.get<LinkTokenResponse>(`${ this.baseUrl }/link-token`)
        .pipe(map(data => data.token));
    }
}