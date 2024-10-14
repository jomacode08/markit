import { PrimengModule } from './../../../shared/primeng/primeng.module';
import { CommonModule } from '@angular/common';
import { Component, CUSTOM_ELEMENTS_SCHEMA, HostListener, inject, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';

import { GoogleOAuthService } from './../../services/googleOAuth.service';
import { SharedModule } from "../../../shared/shared.module";
import { ValidatorErrorField } from '../../../shared/utils/validator-error-field';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { AuthRequest } from '../../interfaces/auth-request';
import { ValidatorService } from '../../../shared/service/validator.service';

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
  private authService = inject(AuthService);
  private googleOAuthService = inject(GoogleOAuthService);
  private router = inject(Router);
  private validatorService = inject(ValidatorService);

  public form = new FormGroup({
    email:       new FormControl<string>('', [Validators.required, Validators.maxLength(320), Validators.pattern(this.validatorService.emailPattern)]),
    password:    new FormControl<string>('', [Validators.required]),
  });
  
  public submit = false;
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

  public onGoogleLogin(): void {
    const googleWindow = this.showGoogleWindow();
    this.setSubmit(true);

    // Check if the window is closed
    const intervalId = setInterval(() => {
      if (googleWindow.closed) {
        clearInterval(intervalId);
        this.setSubmit(false);
      }
    }, 100);
  }

  public onLogin(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.setSubmit(true);
    this.login();
  }

  private login(): void {
    this.authService.login(this.authRequest)
    .subscribe({
        next : () => {
            this.router.navigate(['dashboard']);
            this.setSubmit(false);
        },
        error : () => this.setSubmit(false)
    });
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
    const popUpWidth = 450;
  
    const dualScreenLeft = window.screenLeft ?? window.screenX;
    const dualScreenTop = window.screenTop ?? window.screenY;
    const width = window.innerWidth ?? document.documentElement.clientWidth ?? screen.width;
    const height = window.innerHeight ?? document.documentElement.clientHeight ?? screen.height;
  
    const systemZoom = width / window.screen.availWidth;
    const left = (width - popUpWidth) / 2 / systemZoom + dualScreenLeft;
    const top = (height - popUpHeight) / 2 / systemZoom + dualScreenTop;
  
    const windowFeatures = `width=${ popUpWidth / systemZoom }, height=${ popUpHeight / systemZoom }, top=${ top }, left=${ left }`
    return window.open(this.googleOAuthService.permissionServerUrl(), '_blank', windowFeatures)!;
  }

  @HostListener('window:message', ['$event'])
  private onGoogleAuthCompleted(event: MessageEvent): void {
    this.router.navigate(['dashboard/home']);
  }
}
