import { ChangeDetectionStrategy, Component, DestroyRef, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { InputTextModule } from 'primeng/inputtext';
import { Router } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { ButtonModule } from 'primeng/button';
import { DynamicDialogRef } from 'primeng/dynamicdialog';

import { AuthService } from '../../services/auth.service';
import { ErrorFieldComponent } from '../../../shared/components/layout/error-field/error-field.component';
import { PixelNotepadComponent } from '../../../shared/components/ui/pixel-notepad/pixel-notepad.component';
import { ValidatorErrorField } from '../../../shared/utils/validator-error-field';
import { ValidatorService } from '../../../shared/services/validator.service';

@Component({
  imports: [
    ButtonModule,
    CommonModule,
    ErrorFieldComponent,
    InputTextModule,
    PixelNotepadComponent,
    ReactiveFormsModule
  ],
  templateUrl: './demo-dialog.component.html',
  styleUrl: './demo-dialog.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DemoDialog extends ValidatorErrorField {
  protected submit = signal<boolean>(false);

  public form: FormGroup<{
    guestName: FormControl<string>,
  }>;

  constructor(
    private authService: AuthService,
    private fb: FormBuilder,
    private router: Router,
    private ref : DynamicDialogRef,
    private destroyRef: DestroyRef,
    private validatorService: ValidatorService,
  ) {
    super();
    this.form = fb.nonNullable.group({
      guestName : ['', [
        Validators.required,
        Validators.maxLength(50),
        Validators.pattern(validatorService.internationalNameRegex)]
      ]
    });
  }

  onSubmit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.submit.set(true);
    this.demoLogin();
  }

  
  onCancel() {
    this.ref.close();
  }
  
  private demoLogin() {
    this.authService.demo(this.form.getRawValue().guestName)
    .pipe(takeUntilDestroyed(this.destroyRef))
    .subscribe({
        next : () => this.navigateToDashboard(),
        error : () => this.submit.set(false)
    });
  }

  private navigateToDashboard(): void {
    this.ref.close();
    this.router.navigate(['dashboard']);
  }
}
