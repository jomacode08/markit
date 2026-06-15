import { CommonModule } from '@angular/common';
import { Component, signal, ChangeDetectionStrategy, computed } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize, Observable, tap } from 'rxjs';

import { ButtonModule } from 'primeng/button';
import { DividerModule } from 'primeng/divider';
import { InputText } from 'primeng/inputtext';

import { Account } from '../../../settings/interfaces/account';
import { AccountService } from '../../../settings/services/account.service';
import { SigninMethodsComponent } from './components/signin-methods/signin-methods.component';
import { ValidatorErrorField } from '../../../shared/utils/validator-error-field';
import { CustomMessageService } from '../../../shared/services/custom-message.service';
import { ErrorFieldComponent } from '../../../shared/components/layout/error-field/error-field.component';
import { ValidatorService } from '../../../shared/services/validator.service';

@Component({
  selector: 'app-profile',
  imports: [
    ButtonModule,
    CommonModule,
    DividerModule,
    ErrorFieldComponent,
    InputText,
    ReactiveFormsModule,
    SigninMethodsComponent,
  ],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProfileComponent extends ValidatorErrorField {
  protected account$: Observable<Account>;
  protected account = signal<Account | undefined>(undefined);
  protected submit = signal<boolean>(false);
  protected isEditEnabled = signal<boolean>(false);
  protected roles = computed<string | undefined>(
    () => this.account()?.roles.join(',')
  );

  public form: FormGroup<{
    name: FormControl<string>,
    userName: FormControl<string>,
  }>;

  constructor(
    private accountService: AccountService,
    private fb: FormBuilder,
    private messageService: CustomMessageService,
    private validatorService: ValidatorService,
  ) {
    super();
    this.form = fb.nonNullable.group({
      name: ['', [
        Validators.required,
        Validators.maxLength(50),
        Validators.pattern(validatorService.internationalNameRegex)]
      ],
      userName: ['', [
        Validators.required,
        Validators.maxLength(256),
        Validators.pattern(validatorService.emailPattern)]
      ],
    });
    this.account$ = this.accountService
      .getByCurrentSession()
      .pipe(
        tap((account) => this.account.set(account)),
        tap((account) => this.form.reset(account))
      );
  }

  onSubmit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.submit.set(true);
    this.editAccount();
  }

  onEditBtnClick() {
    this.isEditEnabled.set(true);
  }

  onCancelEditBtnClick() {
    this.form.reset(this.account());
    this.isEditEnabled.set(false);
  }

  private editAccount(): void {
    const formValue = this.form.getRawValue();
    this.accountService.updateMyProfile(formValue.name, formValue.userName)
    .pipe(finalize(() => this.submit.set(false)))
    .subscribe({
      next: (account) => {
        this.account.set(account);
        this.isEditEnabled.set(false);
        this.messageService.showGeneralSuccess("The account was updated successfully.")
      },
      error: (error) => console.error('Error updating account:', error)
    });
  }
}
