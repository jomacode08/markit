import { Injectable, Injector } from '@angular/core';
import { Router } from '@angular/router';
import { HttpInterceptor, HttpEvent, HttpHandler, HttpRequest, HttpErrorResponse } from '@angular/common/http';
import { BehaviorSubject, catchError, filter, Observable, switchMap, take, throwError } from 'rxjs';

import { environment } from '../../../environments/environment';
import { AuthService } from './auth.service';
import { ErrorException, HttpStatusCode, ValidationError } from '../interfaces/error-exception';
import { CustomMessage, MessageType } from '../../shared/interfaces/message/custom-message.interface';
import { CustomMessageService } from '../../shared/services/custom-message.service';
import { TokenService } from './token.service';

@Injectable()
export class HttpRequestInterceptor implements HttpInterceptor {
  constructor(
    private injector: Injector,
    private router: Router,
    private toast: CustomMessageService,
  ) {}

  private readonly GENERAL_ERROR_MESSAGE = "Something went wrong, we keep track of this error, but feel free to contact us if refreshing doesn't fix things.";
  private readonly TOKEN_REFRESH_ENDPOINT = 'auth/token/refresh';
  private isRefreshing : boolean = false;
  private refreshTokenSubject = new BehaviorSubject<boolean | null>(null);

  public intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const authService = this.injector.get(AuthService);
    const tokenService = this.injector.get(TokenService);
    // Evaluate if the hostname is from a 3rd party
    if (!request.url.includes(environment.baseApiUrl)) return next.handle(request);
    
    // Set headers to the request
    request = request.clone(
      {
        withCredentials: true,
        setHeaders: {
          'Content-Type'  : 'application/json, text/plain',
        }
      }
    );
    // Catching errors
    return next.handle(request).pipe(
      catchError((httpError: HttpErrorResponse) => {
        // Mapping httpError
        const error = httpError.error as ErrorException;
        const statusCode = error ? error.statusCode : httpError.status;

        // Show error in console for develop environment
        if (!environment.production) {
          console.log(httpError);
        }

        switch (statusCode) {
          case HttpStatusCode.Unauthorized: {
            if (this.isRefreshing && request.url.includes(this.TOKEN_REFRESH_ENDPOINT)) break;
            return this.handle401Error(
              request,
              next,
              authService,
              tokenService
            );
          }

          case HttpStatusCode.BadRequest: {
            this.showValidationMessage(error.errors);
            break;
          }

          case HttpStatusCode.ServiceUnavailable:
            window.location.reload();
            break;

          case HttpStatusCode.Forbidden:
            this.router.navigate(['/auth/unauthorized'])
            break;

          case HttpStatusCode.InternalServerError:
            this.showErrorMessage(this.GENERAL_ERROR_MESSAGE);
            break;

          default:
          {
            this.showErrorMessage(error.message ?? this.GENERAL_ERROR_MESSAGE);
            break;
          }
        }
        
        return throwError(() => new Error(error.message));
      })
    );
  }

  /**
   * Handles HTTP 401 Unauthorized errors by attempting to refresh the authentication token.
   * If a token refresh is not already in progress, initiates a new refresh request and retries
   * the original request upon successful refresh. If a refresh is already in progress, waits for
   * it to complete before retrying the original request.
   * @returns An observable that emits the HTTP response from the retried request, or an error
   * if the token refresh fails.
   * @remarks
   * Uses a {@link BehaviorSubject} to coordinate multiple concurrent 401 errors and ensure
   * only one token refresh request is made. Additional requests wait for the refresh to complete
   * before retrying.
   * On refresh failure, the user session is invalidated and the error is propagated.
  */
  private handle401Error(
    request: HttpRequest<any>,
    next: HttpHandler,
    authService: AuthService,
    tokenService: TokenService
  ): Observable<HttpEvent<any>>
  {
    if (!this.isRefreshing) {
      this.isRefreshing = true;
      this.refreshTokenSubject.next(null);

      return tokenService.refresh().pipe(
        switchMap(() => {
          this.isRefreshing = false;
          this.refreshTokenSubject.next(true);
          return next.handle(request);
        }),
        catchError((err) => {
          this.isRefreshing = false;
          authService.invalidateSession();
          return throwError(() => err)
        })
      );
    } else {
      return this.refreshTokenSubject.pipe(
        filter(result => result != null),
        take(1),
        switchMap(() => next.handle(request))
      );
    }
  }

  private showErrorMessage(messageBody: string): void {
    const message: CustomMessage = {
      type: MessageType.error,
      title: 'Please verify:',
      message: messageBody
    };
    this.toast.showCustom(message);
  }

  private showValidationMessage(validations: ValidationError): void {
    const message: CustomMessage = {
      type: MessageType.warn,
      title: 'Please verify:',
      validations: validations
    };
    this.toast.showValidations(message);
  }
}