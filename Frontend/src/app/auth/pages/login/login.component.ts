import { CommonModule } from '@angular/common';
import { Component, CUSTOM_ELEMENTS_SCHEMA, HostListener, inject } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';

import { PasswordModule } from 'primeng/password';

import { AuthRequest } from '../../interfaces/auth-request';
import { AuthService } from '../../services/auth.service';
import { ValidatorErrorField } from '../../../shared/utils/validator-error-field';
import { ValidatorService } from '../../../shared/services/validator.service';
import { CustomMessageService } from '../../../shared/services/custom-message.service';
import { GeneralButtonComponent } from '../../../shared/components/ui/buttons/general-button.component';
import { ErrorFieldComponent } from '../../../shared/components/layout/error-field/error-field.component';
import { environment } from '../../../../environments/environment';
import { AuthResponse } from '../../interfaces/auth-response';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    GeneralButtonComponent,
    ErrorFieldComponent,
    PasswordModule,
  ],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent extends ValidatorErrorField {
  private authService        = inject(AuthService);
  private router             = inject(Router);
  private validatorService   = inject(ValidatorService);
  private messageService     = inject(CustomMessageService);
  
  private readonly POPUP_WINDOW_SIZES = {
    height: 600,
    width: 450,
  };

  public form = new FormGroup({
    email:       new FormControl<string>('', [Validators.required, Validators.maxLength(320), Validators.pattern(this.validatorService.emailPattern)]),
    password:    new FormControl<string>('', [Validators.required]),
  });
  
  //* Configuration
  private readonly GOOGLE_AUTH_START_URL = `${ environment.baseApiUrl }/external-login/initiate-google`;
  private popUpWindow : Window | null = null;
  public submit: boolean = false;
  public showLoginForm: boolean = false;

  public get authRequest(): AuthRequest {
    return this.form.value as AuthRequest;
  }

  //* Events
  @HostListener('window:message', ['$event'])
  private listenForExternalAuthResponse(event: MessageEvent): void {
    const authResponse = event.data as AuthResponse;
    this.authService.externalAuthLogin(authResponse);
    this.confirmSession();
    this.popUpWindow?.close();
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
    this.showAuthenticationPopup(this.GOOGLE_AUTH_START_URL);
  }

  //* Methods
  private login(): void {
    this.authService.login(this.authRequest)
    .subscribe({
        next : () => this.confirmSession(),
        error : () => this.setSubmit(false)
    });
  }

  private confirmSession(): void {
    this.setSubmit(false);
    this.router.navigate(['dashboard']);
    this.messageService.showGeneralSuccess("Successful login!");
  }

  //* Utils
  private setSubmit(state: boolean): void {
    this.submit = state;
  }

  private buildCenteredPopupParams(): string {
    const { width, height } = this.POPUP_WINDOW_SIZES;

    const dualScreenLeft = window.screenLeft ?? window.screenX;
    const dualScreenTop  = window.screenTop ?? window.screenY;
    const screenWidth    = window.innerWidth ?? document.documentElement.clientWidth ?? screen.width;
    const windowHeight   = window.innerHeight ?? document.documentElement.clientHeight ?? screen.height;
  
    const systemZoom = screenWidth / window.screen.availWidth;
    const left       = (screenWidth - width) / 2 / systemZoom + dualScreenLeft;
    const top        = (windowHeight - height) / 2 / systemZoom + dualScreenTop;
  
    return `width=${ width / systemZoom }, height=${ height / systemZoom }, top=${ top }, left=${ left }`;
  }

  private showAuthenticationPopup(url: string): void {
    const windowFeatures = this.buildCenteredPopupParams();
    this.popUpWindow = window.open(url, '_blank', windowFeatures);
    this.setSubmit(true);
    // Check if the window is closed
    const intervalId = setInterval(() => {
      if (this.popUpWindow?.closed) {
        clearInterval(intervalId);
        this.setSubmit(false);
      }
    }, 100);
  }
}
