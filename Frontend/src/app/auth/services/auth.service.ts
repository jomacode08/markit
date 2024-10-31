import { catchError, Observable, of, tap } from 'rxjs';
import { computed, Injectable, signal } from '@angular/core';
import { environment } from '../../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';

import { JwtHelperService } from '@auth0/angular-jwt';

import { AuthRequest } from '../interfaces/auth-request';
import { AuthResponse } from '../interfaces/auth-response';
import { AuthStatus } from '../interfaces/auth-status.enum';
import { GeneralConstant } from '../../shared/utils/general-constant';
import { GoogleAuthRequest } from '../interfaces/google/google-auth-request';
import { UserInfo } from './../interfaces/user-info';

@Injectable({ providedIn: 'root' })
export class AuthService {
  //* === Configuration === *//
  private tokenKey: string = GeneralConstant.token_storage_key;
  private _token       = signal<string | null>(null);
  private _currentUser = signal<UserInfo | null>(null);
  private _authStatus  = signal<AuthStatus>(AuthStatus.checking);

  //! To the external world
  public token       = computed( () => this._token() );
  public currentUser = computed( () => this._currentUser() );
  public authStatus  = computed( () => this._authStatus() );

  constructor(
    private jwtHelper: JwtHelperService,
    private http: HttpClient,
    private router: Router
  ) 
  {
    this.checkAuthStatus();
  }

  //* === Login Methods === *//
  public login(authRequest: AuthRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${ environment.baseApiUrl }/login/authenticate`, authRequest)
      .pipe(
        tap(({ token }) => this.setAuthentication(token))
      );
  }

  public loginByGoogle(googleTokenId: string): Observable<AuthResponse> {
    const googleSignRequest: GoogleAuthRequest = {
      tokenId: googleTokenId
    };
    return this.http.post<AuthResponse>(`${ environment.baseApiUrl }/login/authenticateByGoogle`, googleSignRequest)
      .pipe(
        tap(({ token }) => this.setAuthentication(token))
      );
  }

  public logout(): void {
    this._token.set(null);
    this._currentUser.set(null);
    this._authStatus.set(AuthStatus.notAuthenticated);
    localStorage.removeItem(this.tokenKey);
    this.router.navigate(['auth']);
  }

  public isTokenAvailable(): boolean {
    return this._token() != null
    && !this.jwtHelper.isTokenExpired(this._token())
  }

  //* === Utilities ===  //
  private setAuthentication(token: string): void {
    this._token.set(token);
    this._currentUser.set(this.getUserInfo(token));
    this._authStatus.set(AuthStatus.authenticated);
    this.setTokenInLocalStorage(token);
  }

  private checkAuthStatus(): void {
    this._token.set(localStorage.getItem(this.tokenKey));

    this._currentUser.set
    (
      this.isTokenAvailable()
      ? this.getUserInfo(this._token()!)
      : null
    );

    this._authStatus.set
    (
      this.isTokenAvailable()
      ? AuthStatus.authenticated
      : AuthStatus.notAuthenticated
    );
  }

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
}