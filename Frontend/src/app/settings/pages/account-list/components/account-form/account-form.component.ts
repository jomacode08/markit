import { Component, OnInit, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators, AbstractControl, ValidationErrors, ValidatorFn, FormBuilder, AbstractControlOptions } from '@angular/forms';

import { ButtonModule } from 'primeng/button';
import { DynamicDialogConfig, DynamicDialogRef } from 'primeng/dynamicdialog';
import { InputTextModule } from 'primeng/inputtext';
import { MultiSelectModule } from 'primeng/multiselect';
import { PasswordModule } from 'primeng/password';
import { ToggleSwitchModule } from 'primeng/toggleswitch';

import { Account, PasswordRequest } from '../../../../interfaces/account';
import { AccountService } from '../../../../services/account.service';
import { AuthRole } from '../../../../../auth/interfaces/auth-role.enum';
import { ErrorFieldComponent } from '../../../../../shared/components/layout/error-field/error-field.component';
import { ValidatorErrorField } from '../../../../../shared/utils/validator-error-field';
import { ValidatorService } from '../../../../../shared/services/validator.service';

interface RoleOption {
  label : string,
  value : AuthRole,
}

@Component({
    selector: 'app-account-form',
    imports: [
      ButtonModule,
      ErrorFieldComponent,
      InputTextModule,
      MultiSelectModule,
      PasswordModule,
      ReactiveFormsModule,
      ToggleSwitchModule,
    ],
    templateUrl: './account-form.component.html',
    styleUrl: './account-form.component.css'
})
export class AccountFormComponent extends ValidatorErrorField implements OnInit {
  public form : FormGroup<{
    userId : FormControl<string>,
    name : FormControl<string>,
    userName : FormControl<string>,
    roles : FormControl<string[]>,
    password : FormControl<string>,
    confirmPassword : FormControl<string>,
    enabled : FormControl<boolean>
  }>;

  public roleOptions : RoleOption[] = [
    { label: 'Admin', value: AuthRole.ADMIN },
    { label: 'General', value: AuthRole.GENERAL },
    { label: 'Guest', value: AuthRole.GUEST },
  ];
  
  public submit = signal<boolean>(false);
  public isUpdateMode: boolean = false;

  constructor(
    private accountService: AccountService,
    private validatorService: ValidatorService,
    private ref: DynamicDialogRef,
    private config: DynamicDialogConfig,
    private fb : FormBuilder
  ) {
    super();
    this.form = this.fb.nonNullable.group({
      userId: [''],
      name: ['', [Validators.required, Validators.maxLength(256)]],
      userName: ['', [
        Validators.required,
        Validators.pattern(this.validatorService.emailPattern),
        Validators.maxLength(256)
      ]],
      roles: [([] as string[]), [this.atLeastOneRoleValidator()]],
      password: ['', [
        this.requiredAtCreation(),
        Validators.minLength(8),
        Validators.pattern(this.validatorService.passwordPattern),
      ]],
      confirmPassword: [''],
      enabled: [true]
    },
    {
      validators:
      [
        this.validatorService.isTwoFieldsEquals('password','confirmPassword'),
      ]
    } as AbstractControlOptions);
  }

  ngOnInit(): void {
    const accountData = this.config.data as Account | undefined;
    if (accountData?.userId) {
      this.isUpdateMode = true;
      this.form.reset(accountData);
    }
  }

  public onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submit.set(true);
    const accountData : Account = this.form.getRawValue();
    const operation = this.isUpdateMode
      ? this.accountService.update(accountData)
      : this.accountService.create(
        accountData,
        this.constructPasswordRequest(this.form.controls.password.value)
      );

    operation.subscribe({
      next: (result) => {
        this.ref.close(result);
      },
      error: (error) => {
        console.error('Error saving account:', error);
        this.submit.set(false);
      }
    });
  }

  public onCancel(): void {
    this.ref.close();
  }

  private atLeastOneRoleValidator(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const value = control.value;
      return value && value.length > 0 ? null : { required: true };
    };
  }

  private requiredAtCreation(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (this.isUpdateMode) return null;
      const value = control.value;
      return value && value.length > 0 ? null : { required: true };
    }
  }

  private constructPasswordRequest(password: string): PasswordRequest {
    return {
      currentPassword : password,
      newPassword : password 
    } as PasswordRequest;
  }
}