import { ChangeDetectionStrategy, Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AbstractControl, FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

import { ButtonModule } from 'primeng/button';
import { DynamicDialogConfig, DynamicDialogRef } from 'primeng/dynamicdialog';
import { InputTextModule } from 'primeng/inputtext';

import { ErrorFieldComponent } from '../../../../shared/components/layout/error-field/error-field.component';
import { ValidatorErrorField } from '../../../../shared/utils/validator-error-field';
import { isWebUrl, normalizeWebUrl } from '../../../utils/link-url';

export interface LinkDialogData {
  text: string;
  url: string;
}

export interface LinkDialogCloseResponse {
  text: string;
  href: string;
}

@Component({
  selector: 'notebooks-link-dialog',
  imports: [
    ButtonModule,
    CommonModule,
    ErrorFieldComponent,
    InputTextModule,
    ReactiveFormsModule,
  ],
  templateUrl: './link-dialog.component.html',
  styleUrl: './link-dialog.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LinkDialogComponent extends ValidatorErrorField {
  public form: FormGroup<{
    text: FormControl<string>;
    url: FormControl<string>;
  }>;

  constructor(
    private fb: FormBuilder,
    private config: DynamicDialogConfig,
    private ref: DynamicDialogRef,
  ) {
    super();

    const data = this.config.data?.shared as Partial<LinkDialogData> | undefined;

    this.form = this.fb.group({
      text: this.fb.nonNullable.control(data?.text ?? '', [
        Validators.required,
        (control: AbstractControl<string>) => control.value.trim() ? null : { required: true }
      ]),
      url: this.fb.nonNullable.control(data?.url ?? '', [
        Validators.required,
        (control: AbstractControl<string>) => isWebUrl(control.value) ? null : { pattern: true },
      ]),
    });
  }

  public onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const response: LinkDialogCloseResponse = {
      text: value.text.trim(),
      href: normalizeWebUrl(value.url),
    };

    this.ref.close(response);
  }

  public onCancel(): void {
    this.ref.close();
  }
}