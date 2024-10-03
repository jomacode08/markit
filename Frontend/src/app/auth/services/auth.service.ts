import { Injectable } from '@angular/core';

import { JwtHelperService } from '@auth0/angular-jwt';

import { GeneralConstant } from '../../shared/utils/general-constant';
import { Observable, tap } from 'rxjs';
import { AuthResponse } from '../interfaces/auth-response';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { GoogleAuthRequest } from '../interfaces/google/google-auth-request';


@Injectable({ providedIn: 'root' })
export class AuthService {

  constructor(
    private jwtHelper: JwtHelperService,
    private http: HttpClient
  ) { }

  private tokenKey: string = GeneralConstant.token_key;

  get token(): string | undefined {
    const token = localStorage.getItem(this.tokenKey);
    if (!token) return undefined;
    return token;
  }

  public loginByGoogle(googleTokenId: string): Observable<AuthResponse> {
    const googleSignRequest: GoogleAuthRequest = {
      tokenId: googleTokenId
    };
    return this.http.post<AuthResponse>(`${environment.baseApiUrl}/login/authenticateByGoogle`, googleSignRequest)
      .pipe(
        tap(({ token }) => this.setToken(token))
      );
  }

  public isAuthenticated(): boolean {
    return this.token != undefined && this.isTokenAvailable();
  }

  public logout(): void {
    localStorage.removeItem(this.tokenKey);
  }

  private setToken(token: string): void {
    localStorage.setItem(this.tokenKey, token);
  }

  private isTokenAvailable(): boolean {
    if (!this.token) return false;

    if (this.jwtHelper.isTokenExpired(this.token)) {
      this.logout();
      return false;
    }

    return true;
  }
}