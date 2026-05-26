import { CommonModule } from '@angular/common';
import { Component, signal, ChangeDetectionStrategy } from '@angular/core';
import { Observable, tap } from 'rxjs';

import { ButtonModule } from 'primeng/button';
import { DividerModule } from 'primeng/divider';

import { SigninMethodsComponent } from './components/signin-methods/signin-methods.component';
import { AccountService } from '../../../settings/services/account.service';
import { Account } from '../../../settings/interfaces/account';

@Component({
    selector: 'app-profile',
    imports: [
    ButtonModule,
    CommonModule,
    DividerModule,
    SigninMethodsComponent
],
    templateUrl: './profile.component.html',
    styleUrl: './profile.component.css',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProfileComponent {
  protected account = signal<Account | undefined>(undefined);
  protected account$: Observable<Account>;

  constructor(
    private accountService: AccountService
  ) {
    this.account$ = this.accountService
    .getByCurrentSession()
    .pipe(
      tap((account) => this.account.set(account))
    );
  }

  public convertToLargerImageUrl(url: string): string | null {
    return url.replace("s96-c", "s300-c");
  }
}
