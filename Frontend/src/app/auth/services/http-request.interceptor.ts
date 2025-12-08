import { Injectable, Injector } from '@angular/core';
import { Router } from '@angular/router';
import { HttpInterceptor, HttpEvent, HttpHandler, HttpRequest, HttpErrorResponse } from '@angular/common/http';
import { catchError, Observable, throwError } from 'rxjs';

import { environment } from '../../../environments/environment';
import { AuthService } from './auth.service';
import { ErrorException, HttpStatusCode, ValidationError } from '../interfaces/error-exception';
import { CustomMessage, MessageType } from '../../shared/interfaces/message/custom-message.interface';
import { CustomMessageService } from '../../shared/services/custom-message.service';

@Injectable()
export class HttpRequestInterceptor implements HttpInterceptor {
  constructor(
    private injector: Injector,
    private router: Router,
    private toast: CustomMessageService,
  ) {}

  private readonly generalError = "Something went wrong, we keep track of this error, but feel free to contact us if refreshing doesn't fix things.";

  public intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const authService = this.injector.get(AuthService);
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
            authService.invalidateSession();
            break;
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
            this.showErrorMessage(this.generalError);
            break;

          default:
          {
            this.showErrorMessage(error.message ?? this.generalError);
            break;
          }
        }
        
        return throwError(() => new Error(error.message));
      })
    );
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