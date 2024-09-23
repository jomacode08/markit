import { CommonModule } from '@angular/common';
import { Component, CUSTOM_ELEMENTS_SCHEMA, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Params } from '@angular/router';
import { catchError, delay, firstValueFrom } from 'rxjs';

import { AuthService } from './../../services/auth.service';
import { GoogleOAuthService } from './../../services/googleOAuth.service';
import { SharedModule } from '../../../shared/shared.module';

@Component({
  selector: 'app-status',
  standalone: true,
  imports: [
    CommonModule,
    SharedModule
],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  templateUrl: './status.component.html'
})
export class StatusComponent implements OnInit {
  
  public activatedRoute = inject(ActivatedRoute);
  public authService = inject(AuthService);
  public googleOAuthService = inject(GoogleOAuthService);

  public loading: boolean = true;
  get isAuthenticated(): boolean {
    return this.authService.isAuthenticated();
  }

  public async ngOnInit(): Promise<void> {
    const params = await this.getUrlParams();

    if (params['code'] == null) {
      this.loading = false;
      return;
    }
    
    // In case of receive the authorization code from google, proceed to the google login process
    this.googleOAuthService.login(params['code'])
    .pipe(
      delay(1500),
      catchError(error => {
        this.setLoading(false);
        throw new Error(error);
      }),
    ).subscribe(() => {
      this.setLoading(false);
      // Comunicate the authentication is completed to the login window
      // The addition of the dalay 500ms is for user experience purposes
      setTimeout(() => {
        window.opener.postMessage('auth-completed');
        window.close();
      }, 800);
    });
  }

  private async getUrlParams(): Promise<Params> {
    return firstValueFrom(this.activatedRoute.queryParams);
  }

  private setLoading( state: boolean ): void {
    this.loading = state;
  }
}
