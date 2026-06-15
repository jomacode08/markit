import { ChangeDetectionStrategy, Component, DestroyRef, signal } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { ButtonModule } from 'primeng/button';
import { DynamicDialogRef } from 'primeng/dynamicdialog';
import { PasswordModule } from 'primeng/password';

import { AuthService } from '../../../../../auth/services/auth.service';
import { ErrorFieldComponent } from '../../../../../shared/components/layout/error-field/error-field.component';
import { ValidatorErrorField } from '../../../../../shared/utils/validator-error-field';
import { ValidatorService } from '../../../../../shared/services/validator.service';
import { CustomMessageService } from '../../../../../shared/services/custom-message.service';

@Component({
  selector: 'app-change-password-dialog',
  imports: [
    ButtonModule,
    ErrorFieldComponent,
    PasswordModule,
    ReactiveFormsModule,
  ],
  templateUrl: './change-password-dialog.component.html',
  styleUrl: './change-password-dialog.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ChangePasswordDialogComponent extends ValidatorErrorField {
  private readonly SUCCESS_MESSAGE: string = 'The password was changed successfully.';
  public form: FormGroup<{
    currentPassword: FormControl<string>,
    newPassword: FormControl<string>,
  }>;
  public submit = signal<boolean>(false);

  constructor(
    private authService: AuthService,
    private destroyRef: DestroyRef,
    private fb: FormBuilder,
    private messageService: CustomMessageService,
    private ref: DynamicDialogRef,
    private validatorService: ValidatorService
  ) {
    super();
    this.form = this.fb.nonNullable.group({
      currentPassword: ['', [Validators.required]],
      newPassword: ['', [
        Validators.required,
        Validators.minLength(8),
        Validators.pattern(this.validatorService.passwordPattern),
      ]],
    })
  }

  public onSubmit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const formData = this.form.getRawValue();
    this.submit.set(true);
    this.authService.changeMyPassword(formData.currentPassword, formData.newPassword)
    .pipe(takeUntilDestroyed(this.destroyRef))
    .subscribe({
      next: () => {
        this.messageService.showGeneralSuccess(this.SUCCESS_MESSAGE);
        this.ref.close();
      },
      error: (error) => this.submit.set(false)
    });
  }

  public onCancel() {
    this.ref.close();
  }
}
