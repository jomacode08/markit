import { CommonModule } from '@angular/common';
import { Component, CUSTOM_ELEMENTS_SCHEMA, HostListener, inject, OnDestroy, OnInit } from '@angular/core';
import { delay, switchMap } from 'rxjs';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { PrimengModule } from './../../../shared/primeng/primeng.module';
import { Router } from '@angular/router';

import { AuthRequest } from '../../interfaces/auth-request';
import { AuthService } from '../../services/auth.service';
import { GoogleOAuthService } from './../../services/googleOAuth.service';
import { SharedModule } from "../../../shared/shared.module";
import { ValidatorErrorField } from '../../../shared/utils/validator-error-field';
import { ValidatorService } from '../../../shared/services/validator.service';
import { CustomMessageService } from '../../../shared/services/custom-message.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    SharedModule,
    PrimengModule,
    ReactiveFormsModule
  ],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent extends ValidatorErrorField implements OnInit, OnDestroy {
  private authService        = inject(AuthService);
  private googleOAuthService = inject(GoogleOAuthService);
  private router             = inject(Router);
  private validatorService   = inject(ValidatorService);
  private messageService     = inject(CustomMessageService);
  
  public form = new FormGroup({
    email:       new FormControl<string>('', [Validators.required, Validators.maxLength(320), Validators.pattern(this.validatorService.emailPattern)]),
    password:    new FormControl<string>('', [Validators.required]),
  });
  
  //* Configuration
  private googleAuthWindow : Window | null = null;
  public submit: boolean = false;
  public showLoginForm: boolean = false;
  public mediaTypes: string[] = ["Posts", "Reels", "Videos", "Notes", "Articles", "Code"];
  public mediaCounter: number = 0;
  public mediaIntervalId ?: ReturnType<typeof setTimeout>;

  public get authRequest(): AuthRequest {
    return this.form.value as AuthRequest;
  }
  
  public ngOnInit(): void {
    this.mediaLoader();
  }

  public ngOnDestroy(): void {
    if (this.mediaIntervalId != null) clearInterval(this.mediaIntervalId);
  }

  public onSignIn = (): void => { this.showLoginForm = true; }

  public onLogin(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.setSubmit(true);
    this.login();
  }

  public onGoogleLogin(): void {
    this.googleAuthWindow = this.showGoogleWindow();
    this.setSubmit(true);
    // Check if the window is closed
    const intervalId = setInterval(() => {
      if (this.googleAuthWindow?.closed) {
        clearInterval(intervalId);
        this.setSubmit(false);
      }
    }, 100);
  }

  private login(): void {
    this.authService.login(this.authRequest)
    .subscribe({
        next : () => this.confirmSession(),
        error : () => this.setSubmit(false)
    });
  }

  // In case of receive the authorization code from google, proceed to the internal google login process
  private loginByGoogle( authCode: string ): void {
    this.googleOAuthService.login(authCode)
    .pipe(
      delay(2000),
      switchMap(({ id_token }) => this.authService.loginByGoogle(id_token)),
    ).subscribe({
      next: () => {
        this.confirmSession();
        this.googleAuthWindow?.close();
      },
      error : () => {
        this.setSubmit(false);
        this.googleAuthWindow?.close();
      }
    });
  }
  
  //* Utilities *//
  @HostListener('window:message', ['$event'])
  private onGoogleAuthorizated(event: MessageEvent): void {
    this.loginByGoogle(event.data);
  }

  private setSubmit(state: boolean): void {
    this.submit = state;
  }

  private mediaLoader(): void {
    this.mediaIntervalId = setInterval(() => {
      if (this.mediaCounter === this.mediaTypes.length - 1)
        this.mediaCounter = 0;
      else
        this.mediaCounter++;
    }, 3000);
  }

  private showGoogleWindow(): Window {
    const popUpHeight = 600;
    const popUpWidth  = 450;
  
    const dualScreenLeft = window.screenLeft ?? window.screenX;
    const dualScreenTop  = window.screenTop ?? window.screenY;
    const width          = window.innerWidth ?? document.documentElement.clientWidth ?? screen.width;
    const height         = window.innerHeight ?? document.documentElement.clientHeight ?? screen.height;
  
    const systemZoom = width / window.screen.availWidth;
    const left       = (width - popUpWidth) / 2 / systemZoom + dualScreenLeft;
    const top        = (height - popUpHeight) / 2 / systemZoom + dualScreenTop;
  
    const windowFeatures = `width=${ popUpWidth / systemZoom }, height=${ popUpHeight / systemZoom }, top=${ top }, left=${ left }`
    return window.open(this.googleOAuthService.permissionServerUrl(), '_blank', windowFeatures)!;
  }

  private confirmSession(): void {
    this.setSubmit(false);
    this.router.navigate(['dashboard']);
    this.messageService.showGeneralSuccess("Successful login!");
  }
}
