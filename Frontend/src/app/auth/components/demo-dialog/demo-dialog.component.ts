import { ChangeDetectionStrategy, Component, DestroyRef, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { InputTextModule } from 'primeng/inputtext';

import { ButtonModule } from 'primeng/button';

import { ErrorFieldComponent } from '../../../shared/components/layout/error-field/error-field.component';
import { ValidatorErrorField } from '../../../shared/utils/validator-error-field';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';
import { DynamicDialogRef } from 'primeng/dynamicdialog';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

@Component({
  imports: [
    ButtonModule,
    CommonModule,
    ErrorFieldComponent,
    InputTextModule,
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
    private destroyRef: DestroyRef
  ) {
    super();
    this.form = fb.nonNullable.group({
      guestName : ['', Validators.required]
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
