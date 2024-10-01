import { computed, inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';

import { googleOAuthEnvironment } from '../../../environments/environment';
import { OAuth20Response } from '../interfaces/google-auth-response';
import { AuthService } from './auth.service';

@Injectable({providedIn: 'root'})
export class GoogleOAuthService {

    private http = inject(HttpClient);
    private authService = inject(AuthService);

    private baseUrlPermissionServer = 'https://accounts.google.com/o/oauth2/v2/auth';
    private scopes = 'email https://www.googleapis.com/auth/userinfo.profile';
    public permissionServerUrl = computed(() => 
        this.baseUrlPermissionServer +
        '?access_type=online' +
        `&client_id=${ googleOAuthEnvironment.clientId }` +
        `&redirect_uri=${ googleOAuthEnvironment.redirectUri }` +
        '&response_type=code' +
        `&scope=${ this.scopes }`
    );

    public login(authCode: string): Observable<OAuth20Response> {
        return this.http.post<OAuth20Response>('https://oauth2.googleapis.com/token', null, {
            params: {
                ['client_id'] : googleOAuthEnvironment.clientId,
                ['client_secret'] : googleOAuthEnvironment.clientSecret,
                ['code'] : authCode,
                ['grant_type'] : 'authorization_code',
                ['redirect_uri'] : googleOAuthEnvironment.redirectUri
            }
        }).pipe(
            tap(({ id_token }) => this.authService.setToken(id_token))
        );
    }
}