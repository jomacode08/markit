import { UserInfo } from './../interfaces/user-info';
import { Injectable } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

import { JwtHelperService } from '@auth0/angular-jwt';

import { GeneralConstant } from '../../shared/utils/general-constant';
import { AuthResponse } from '../interfaces/auth-response';
import { GoogleAuthRequest } from '../interfaces/google/google-auth-request';
import { AuthRequest } from '../interfaces/auth-request';
import { Router } from '@angular/router';

@Injectable({ providedIn: 'root' })
export class AuthService {
  // Configuration
  private tokenKey: string = GeneralConstant.token_storage_key;
  private _token: string | null = null;

  public get token(): string | null {
    if (this._token) return this._token;
    return this._token = localStorage.getItem(this.tokenKey);
  }
  
  public get currentUser(): UserInfo | null {
    if (!this._token) return null;
    return this.getUserInfo(this._token);
  }

  constructor(
    private jwtHelper: JwtHelperService,
    private http: HttpClient,
    private router: Router
  ) { }

  public login(authRequest: AuthRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${ environment.baseApiUrl }/login/authenticate`, authRequest)
      .pipe(
        tap(({ token }) => this.initializeSession(token)),
      );
  }

  public loginByGoogle(googleTokenId: string): Observable<AuthResponse> {
    const googleSignRequest: GoogleAuthRequest = {
      tokenId: googleTokenId
    };
    return this.http.post<AuthResponse>(`${ environment.baseApiUrl }/login/authenticateByGoogle`, googleSignRequest)
      .pipe(
        tap(({ token }) => this.initializeSession(token))
      );
  }

  // Validate if the token is available and the user info exists.
  public isAuthenticated(): boolean {
    return this.token != null
    && !this.jwtHelper.isTokenExpired(this.token)
  }

  public logout(): void {
    this._token = null;
    localStorage.removeItem(this.tokenKey);
    this.router.navigate(['auth']);
  }

  // === UTILS ===  //
  private setTokenInLocalStorage(token: string): void {
    localStorage.setItem(this.tokenKey, token);
  }

  private getUserInfo( token: string ): UserInfo {
    const { given_name, prof_pic_url } = this.jwtHelper.decodeToken(token);
    return {
      given_name : given_name,
      userPictureUrl : prof_pic_url
    }
  }

  private initializeSession(token: string): void {
    this._token = token;
    this.setTokenInLocalStorage(token);
  }
}