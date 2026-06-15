import { AsyncPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, input, OnDestroy, signal } from '@angular/core';
import { Observable, startWith, Subject, switchMap } from 'rxjs';

import { ButtonModule } from 'primeng/button';
import { DialogService, DynamicDialogRef } from 'primeng/dynamicdialog';
import { SharedModule } from 'primeng/api';

import { AuthOptions } from '../../../../../auth/interfaces/auth-options';
import { AuthService } from '../../../../../auth/services/auth.service';
import { ChangePasswordDialogComponent } from '../change-password-dialog/change-password-dialog.component';
import { CustomMessageService } from '../../../../../shared/services/custom-message.service';
import { ExternalLoginService } from '../../../../../auth/services/external-login.service';
import { ExternalSignInMethod, LoginProvider, LoginPurpose, SignInMethods } from '../../../../../auth/interfaces/signin-methods';
import { LoginProviderIconPipe } from '../../../../../shared/pipes/login-provider-icon.pipe';
import { POPUP_NAMES } from '../../../../../shared/utils/constant';
import { PopupService } from '../../../../../shared/services/popup.service';
import { RedirectResponse } from '../../../../../auth/interfaces/redirect';

@Component({
    selector: 'profile-signin-methods',
    imports: [
      AsyncPipe,
      ButtonModule,
      LoginProviderIconPipe,
      SharedModule,
    ],
    templateUrl: './signin-methods.component.html',
    styleUrl: './signin-methods.component.css',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class SigninMethodsComponent implements OnDestroy {
  public username = input.required<string>();

  private readonly LOGIN_LINKED_SUCCESS_MESSAGE = "The account was successfully linked.";
  private readonly LOGIN_REMOVED_SUCCESS_MESSAGE = "The account was successfully removed.";
  
  
  private dialogRef?: DynamicDialogRef<ChangePasswordDialogComponent> | null;
  private refreshTrigger$ = new Subject<void>();
  public signInMethods$ : Observable<SignInMethods>;
  public isSubmit = signal<boolean>(false);
  protected authAvailability = signal<AuthOptions>({
    isDemoModeAvailable : false,
    isGitHubAvailable: false,
    isGoogleAvailable: false
  });

  constructor(
    private authService: AuthService,
    private dialogService: DialogService,
    private externalLoginService: ExternalLoginService,
    private popupService: PopupService,
    private messageService: CustomMessageService,
  ) {
    this.checkAuthAvailability();
    this.signInMethods$ = this.refreshTrigger$.pipe(
      startWith(null),
      switchMap(() => authService.getSignInMethods())
    );
  }

  public ngOnDestroy(): void {
    if (this.dialogRef) this.dialogRef.destroy();
  }

  public onConfigureLogin(signInMethod: ExternalSignInMethod): void {
    if (signInMethod.configured) return;
    this.openAuthPopUp(signInMethod.loginProvider);
  }

  public onRemoveLogin(signInMethod: ExternalSignInMethod): void {
    const { loginProvider, identifier } = signInMethod;
    const DIALOG_HEADER = `Remove ${ loginProvider } login`;
    const CONFIRMATION_MESSAGE = `Do you want to unlink your account: ${ identifier }?`;
    this.setSubmit(true);
    this.messageService.showConfirmationDialog({
      header: DIALOG_HEADER,
      message: CONFIRMATION_MESSAGE,
      accept: () => {
        this.removeLogin(loginProvider);
      },
      reject: () => {
        this.setSubmit(false);
      }
    });
  }

  onChangePasswordBtnClick(): void {
    this.dialogRef = this.dialogService.open(ChangePasswordDialogComponent, {
      header: 'Change password',
      width: '500px',
      styleClass: 'custom-dialog',
      closable: true,
    });
  }

  public isLoginProviderAvailable(provider: LoginProvider): boolean {
    switch (provider) {
      case LoginProvider.Google:
        return this.authAvailability().isGoogleAvailable;
      case LoginProvider.GitHub:
        return this.authAvailability().isGitHubAvailable;
      default: return false;
    }
  }

  private openAuthPopUp(provider: LoginProvider): void {
    this.setSubmit(true);
    // Get the initiation url of the login provider. 
    this.externalLoginService.getLoginUrlForProvider(provider, LoginPurpose.LinkAccount)
    .subscribe({
      next: url => {
        // Open a new popup window with the authorization page.
        this.popupService.open({
          name: POPUP_NAMES.LINK_ACCOUNT,
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
    this.popupService.listenForMessagesFrom(POPUP_NAMES.LINK_ACCOUNT).subscribe(
      (event) => {
        const { state } = event.data as RedirectResponse;
        if (state === 'success') {
          this.refreshSignInMethods();
          this.messageService.showGeneralSuccess(this.LOGIN_LINKED_SUCCESS_MESSAGE);
        }
        this.popupService.close(POPUP_NAMES.SIGN_IN);
      }
    );
  }

  private removeLogin(provider: LoginProvider): void {
    this.externalLoginService.remove(provider)
    .subscribe({
      next: () => {
        this.refreshSignInMethods();
        this.messageService.showGeneralSuccess(this.LOGIN_REMOVED_SUCCESS_MESSAGE);
        this.setSubmit(false);
      },
      error: () => this.setSubmit(false)
    });
  }

  private checkAuthAvailability(): void {
    this.authService.getOptions()
    .subscribe((options) => this.authAvailability.set(options));
  }

  private setSubmit = (state: boolean) => this.isSubmit.set(state);
  private refreshSignInMethods = () => this.refreshTrigger$.next();
}
