import { catchError, map, Observable, of, tap } from 'rxjs';
import { computed, Injectable, signal } from '@angular/core';
import { environment } from '../../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';

import { AuthRequest } from '../interfaces/auth-request';
import { AuthStatus } from '../interfaces/auth-status.enum';
import { AuthenticatedUser } from '../interfaces/auth-user';
import { ISAUTHENTICATED_STORAGE_KEY, MARKS_STORAGE_KEY } from '../../shared/utils/constant';
import { SignInMethods } from '../interfaces/signin-methods';

@Injectable({ providedIn: 'root' })
export class AuthService {
  //* === Configuration === *//
  private _currentUser = signal<AuthenticatedUser | null>(null);
  private _authStatus = signal<AuthStatus>(AuthStatus.checking);
  private baseUrl: string = `${environment.baseApiUrl}/login`;

  //! To the external world
  public currentUser = computed(() => this._currentUser());
  public authStatus = computed(() => this._authStatus());

  constructor(
    private http: HttpClient,
    private router: Router,
  ) 
  {}

  public login(authRequest: AuthRequest): Observable<AuthenticatedUser> {
    return this.http.post<AuthenticatedUser>(`${ this.baseUrl }/authenticate`, authRequest)
    .pipe(
      tap((authUser) => this.setAuthentication(authUser))
    );
  }
  
  public logout(): void {
    this.logoutFromApi().subscribe(() => {
      this.unsetAuthentication();
      this.router.navigate(['auth']);
    });
  }

  public isAuthenticated(): Observable<boolean> {
    return this.http.get<AuthenticatedUser>(`${ this.baseUrl }/isAuthenticated`)
    .pipe(
      tap((authUser : AuthenticatedUser) => this.setAuthentication(authUser)),
      map(() => true),
      catchError(() => of(false))
    );
  }

  public getSignInMethods(): Observable<SignInMethods> {
    return this.http.get<SignInMethods>(`${ this.baseUrl }/signin-methods`);
  }

  public invalidateSession(): void {
    this.unsetAuthentication();
    this.router.navigate(['auth']);
  }
  
  public inicializeAuth(): Observable<boolean> {
    const authFlag : string | null = this.getAuthFlag();
    if (authFlag === 'false') return of(false);
    return this.isAuthenticated();
  }

  private logoutFromApi(): Observable<void> {
    return this.http.post<void>(`${ this.baseUrl }/logout`, {});
  }

  //* === Utilities ===  //
  private setAuthentication(authUser: AuthenticatedUser): void {
    this._currentUser.set(authUser);
    this._authStatus.set(AuthStatus.authenticated);
    this.setAuthFlag(true);
  }

  private unsetAuthentication(): void {
    this._currentUser.set(null);
    this._authStatus.set(AuthStatus.notAuthenticated);
    this.setAuthFlag(false);
    localStorage.removeItem(MARKS_STORAGE_KEY);
  }

  private getAuthFlag(): string | null {
    return localStorage.getItem(ISAUTHENTICATED_STORAGE_KEY);
  }
   
  private setAuthFlag(isAuthenticated: boolean) {
    localStorage.setItem(ISAUTHENTICATED_STORAGE_KEY, isAuthenticated ? 'true' : 'false');
  }
}