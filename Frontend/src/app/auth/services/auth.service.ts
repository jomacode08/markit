import { Observable, tap } from 'rxjs';
import { computed, Injectable, signal } from '@angular/core';
import { environment } from '../../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';

import { JwtHelperService } from '@auth0/angular-jwt';

import { AuthRequest } from '../interfaces/auth-request';
import { AuthResponse } from '../interfaces/auth-response';
import { AuthStatus } from '../interfaces/auth-status.enum';
import { UserInfo } from './../interfaces/user-info';
import { MARKS_STORAGE_KEY, TOKEN_STORAGE_KEY } from '../../shared/utils/constant';
import { SignInMethods } from '../interfaces/signin-methods';

@Injectable({ providedIn: 'root' })
export class AuthService {
  //* === Configuration === *//
  private _token = signal<string | null>(null);
  private _currentUser = signal<UserInfo | null>(null);
  private _authStatus = signal<AuthStatus>(AuthStatus.checking);
  private baseUrl: string = `${environment.baseApiUrl}/login`;

  //! To the external world
  public token = computed(() => this._token());
  public currentUser = computed(() => this._currentUser());
  public authStatus = computed(() => this._authStatus());

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
    return this.http.post<AuthResponse>(`${ this.baseUrl }/authenticate`, authRequest)
      .pipe(
        tap((authData) => this.setAuthentication(authData))
      );
  }

  public externalAuthLogin(auth: AuthResponse) {
    this.setAuthentication(auth);
  }

  public logout(): void {
    this.logoutFromApi().subscribe(() => {
      this._token.set(null);
      this._currentUser.set(null);
      this._authStatus.set(AuthStatus.notAuthenticated);
      localStorage.removeItem(TOKEN_STORAGE_KEY);
      localStorage.removeItem(MARKS_STORAGE_KEY);
      this.router.navigate(['auth']);
    });
  }

  public getSignInMethods(): Observable<SignInMethods> {
    return this.http.get<SignInMethods>(`${ this.baseUrl }/signin-methods`);
  }

  public isTokenAvailable(): boolean {
    return this._token() != null
    && !this.jwtHelper.isTokenExpired(this._token())
  }

  //* === Utilities ===  //
  private setAuthentication(auth: AuthResponse): void {
    const { token } = auth;
    this._token.set(token);
    this._currentUser.set(this.getUserInfo(token));
    this._authStatus.set(AuthStatus.authenticated);
    this.setTokenInLocalStorage(token, TOKEN_STORAGE_KEY);
  }

  private checkAuthStatus(): void {
    this._token.set(localStorage.getItem(TOKEN_STORAGE_KEY));

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

  private logoutFromApi(): Observable<void> {
    return this.http.post<void>(`${ this.baseUrl }/logout`, {});
  }

  private setTokenInLocalStorage(token: string, key: string): void {
    localStorage.setItem(key, token);
  }

  private getUserInfo( token: string ): UserInfo {
    const { given_name, prof_pic_url, creatorId, email } = this.jwtHelper.decodeToken(token);
    return {
      given_name : given_name,
      email : email,
      userPictureUrl : prof_pic_url,
      creatorId : creatorId,
    }
  }
}