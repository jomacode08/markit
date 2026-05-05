import { ChangeDetectionStrategy, Component, computed, CUSTOM_ELEMENTS_SCHEMA, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';

import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';

import { AuthOptions } from '../../interfaces/auth-options';
import { AuthRequest } from '../../interfaces/auth-request';
import { AuthService } from '../../services/auth.service';
import { ErrorFieldComponent } from '../../../shared/components/layout/error-field/error-field.component';
import { ExternalLoginService } from '../../services/external-login.service';
import { LoginProvider, LoginPurpose } from '../../interfaces/signin-methods';
import { POPUP_NAMES } from '../../../shared/utils/constant';
import { PopupService } from '../../../shared/services/popup.service';
import { RedirectResponse } from '../../interfaces/redirect';
import { ValidatorErrorField } from '../../../shared/utils/validator-error-field';
import { ValidatorService } from '../../../shared/services/validator.service';

@Component({
    selector: 'app-login',
    imports: [
      ButtonModule,
      CommonModule,
      InputTextModule,
      ReactiveFormsModule,
      ErrorFieldComponent,
      PasswordModule,
    ],
    schemas: [CUSTOM_ELEMENTS_SCHEMA],
    templateUrl: './login.component.html',
    styleUrl: './login.component.css',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class LoginComponent extends ValidatorErrorField implements OnInit {  
  protected submit = signal<boolean>(false);
  protected authOptions = signal<AuthOptions>({
    isDemoModeAvailable : false,
    isGitHubAvailable: false,
    isGoogleAvailable: false
  });
  protected availableOptionsCounter = computed<number>(() =>
    Object.values(this.authOptions()).filter(Boolean).length
  );

  public form : FormGroup<{
    email    : FormControl<string>,
    password : FormControl<string>
  }>;

  public get authRequest(): AuthRequest {
    return this.form.value as AuthRequest;
  }

  constructor(
    private authService : AuthService,
    private externalLoginService : ExternalLoginService,
    private fb : FormBuilder,
    private popupService : PopupService,
    private router : Router,
    private validator : ValidatorService,
  ) {
    super()
    this.form = this.fb.nonNullable.group({
      email    : ['', [Validators.required, Validators.maxLength(320), Validators.pattern(this.validator.emailPattern)]],
      password : ['', Validators.required]
    });
  }

  public ngOnInit(): void {
    this.checkAuthOptions();
  }

  //* Events
  public onLogin(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.setSubmit(true);
    this.login();
  }

  public onGoogleLogin(): void {
    this.openAuthPopUp(LoginProvider.Google);
  }

  public onGithubLogin(): void {
    this.openAuthPopUp(LoginProvider.GitHub);
  }

  public onTryDemo(): void {
    this.setSubmit(true);
    this.authService.demo()
    .subscribe({
        next : () => this.confirmSession(),
        error : () => this.setSubmit(false)
    });
  }

  //* Methods
  private login(): void {
    this.authService.login(this.authRequest)
    .subscribe({
        next : () => this.confirmSession(),
        error : () => this.setSubmit(false)
    });
  }

  private openAuthPopUp(provider: LoginProvider): void {
    this.setSubmit(true);
    // Get the initiation url of the login provider. 
    this.externalLoginService.getLoginUrlForProvider(provider, LoginPurpose.SignIn)
    .subscribe({
      next: url => {
        // Open a new popup window with the authorization page.
        this.popupService.open({
          name: POPUP_NAMES.SIGN_IN,
          url,
          close: () => this.setSubmit(false),
        });
        // Listen for the redirect message response.
        this.listenForAuthRedirect();
      },
      error: () => this.setSubmit(false)
    });
  }

  private listenForAuthRedirect(): void {
    this.popupService.listenForMessagesFrom(POPUP_NAMES.SIGN_IN).subscribe(
      async (event) => {
        const response = event.data as RedirectResponse;
        if (response.purpose != 'sign-in' || response.state === 'failure') return;
        const isAuthenticated = await firstValueFrom(this.authService.isAuthenticated());
        if (isAuthenticated) this.confirmSession();
        this.popupService.close(POPUP_NAMES.SIGN_IN);
      }
    );
  }

  private confirmSession(): void {
    this.setSubmit(false);
    this.router.navigate(['dashboard']);
  }

  private setSubmit(state: boolean): void {
    this.submit.set(state)
  }

  private checkAuthOptions(): void {
    this.authService.getOptions()
    .subscribe((options) => this.authOptions.set(options));
  }
}