import { AsyncPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, signal } from '@angular/core';
import { Observable, startWith, Subject, switchMap, tap } from 'rxjs';

import { SharedModule } from 'primeng/api';

import { GeneralButtonComponent } from "../../../../../shared/components/ui/buttons/general-button.component";
import { AuthService } from '../../../../../auth/services/auth.service';
import { ExternalSignInMethod, LoginProvider, LoginPurpose, SignInMethods } from '../../../../../auth/interfaces/signin-methods';
import { LoginProviderIconPipe } from '../../../../../shared/pipes/login-provider-icon.pipe';
import { ExternalLoginService } from '../../../../../auth/services/external-login.service';
import { PopupService } from '../../../../../shared/services/popup.service';
import { POPUP_NAMES } from '../../../../../shared/utils/constant';
import { RedirectResponse } from '../../../../../auth/interfaces/redirect';
import { CustomMessageService } from '../../../../../shared/services/custom-message.service';

@Component({
    selector: 'profile-signin-methods',
    imports: [
        SharedModule,
        GeneralButtonComponent,
        AsyncPipe,
        LoginProviderIconPipe,
    ],
    templateUrl: './signin-methods.component.html',
    styleUrl: './signin-methods.component.css',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class SigninMethodsComponent {
  private readonly LOGIN_LINKED_SUCCESS_MESSAGE = "The account was successfully linked.";
  private readonly LOGIN_REMOVED_SUCCESS_MESSAGE = "The account was successfully removed.";

  private refresTrigger$ = new Subject<void>();
  public signInMethods$ : Observable<SignInMethods>;
  public currentEmail : string | undefined;
  public isSubmit = signal<boolean>(false);

  constructor(
    private authService: AuthService,
    private externalLoginService: ExternalLoginService,
    private popupService: PopupService,
    private messageService: CustomMessageService,
  ) {
    this.signInMethods$ = this.refresTrigger$.pipe(
      startWith(null),
      switchMap(() => authService.getSignInMethods()),
      tap(() => this.currentEmail = this.authService.currentUser()?.email)
    );
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

  private setSubmit = (state: boolean) => this.isSubmit.update(() => state);
  private refreshSignInMethods = () => this.refresTrigger$.next();
}
