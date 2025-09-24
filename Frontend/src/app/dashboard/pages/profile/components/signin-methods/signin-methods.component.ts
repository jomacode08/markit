import { AsyncPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, type OnInit } from '@angular/core';
import { Observable, tap } from 'rxjs';

import { SharedModule } from 'primeng/api';

import { GeneralButtonComponent } from "../../../../../shared/components/ui/buttons/general-button.component";
import { AuthService } from '../../../../../auth/services/auth.service';
import { ExternalSignInMethod, LoginProvider, SignInMethods } from '../../../../../auth/interfaces/signin-methods';

@Component({
  selector: 'profile-signin-methods',
  standalone: true,
  imports: [
    SharedModule,
    GeneralButtonComponent,
    AsyncPipe
],
  templateUrl: './signin-methods.component.html',
  styleUrl: './signin-methods.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SigninMethodsComponent {

  public signInMethods$ : Observable<SignInMethods>;
  public currentEmail : string | undefined;
  public googleMethod : ExternalSignInMethod | undefined;

  constructor(private authService: AuthService) {
    this.signInMethods$ = authService.getSignInMethods()
    .pipe(
      tap((data: SignInMethods) => this.setUpSignInMethods(data))
    );
  }
  
  private setUpSignInMethods(signInMethods: SignInMethods): void {
    const { externalSignMethods } = signInMethods;
    this.currentEmail = this.authService.currentUser()?.email;
    this.googleMethod = externalSignMethods.find(e => e.loginProvider === LoginProvider.Google);
  }

}
