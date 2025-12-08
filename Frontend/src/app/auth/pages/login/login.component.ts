import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, CUSTOM_ELEMENTS_SCHEMA, signal } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';

import { PasswordModule } from 'primeng/password';

import { AuthRequest } from '../../interfaces/auth-request';
import { AuthService } from '../../services/auth.service';
import { CustomMessageService } from '../../../shared/services/custom-message.service';
import { ErrorFieldComponent } from '../../../shared/components/layout/error-field/error-field.component';
import { ExternalLoginService } from '../../services/external-login.service';
import { GeneralButtonComponent } from '../../../shared/components/ui/buttons/general-button.component';
import { LoginProvider, LoginPurpose } from '../../interfaces/signin-methods';
import { POPUP_NAMES } from '../../../shared/utils/constant';
import { PopupService } from '../../../shared/services/popup.service';
import { RedirectResponse } from '../../interfaces/redirect';
import { ValidatorErrorField } from '../../../shared/utils/validator-error-field';
import { ValidatorService } from '../../../shared/services/validator.service';
import { firstValueFrom } from 'rxjs';

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
  styleUrl: './login.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginComponent extends ValidatorErrorField {  
  public submit = signal<boolean>(false);
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
    private messageService: CustomMessageService,
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
    this.messageService.showGeneralSuccess("Successful login!");
  }

  private setSubmit(state: boolean): void {
    this.submit.set(state)
  }
}
