import { ChangeDetectionStrategy, Component, EventEmitter, Output } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';

import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { SharedModule } from 'primeng/api';

import { ValidatorErrorField } from '../../../../../shared/utils/validator-error-field';
import { ErrorFieldComponent } from "../../../../../shared/components/layout/error-field/error-field.component";
import { ValidatorService } from '../../../../../shared/services/validator.service';

@Component({
    selector: 'gist-loader',
    imports: [
      ButtonModule,
      CommonModule,
      ErrorFieldComponent,
      InputTextModule,
      ReactiveFormsModule,
      SharedModule,
    ],
    templateUrl: './gist-loader.component.html',
    styleUrl: './gist-loader.component.css',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class GistLoaderComponent extends ValidatorErrorField {
  @Output()
  private onGistSelected = new EventEmitter<string>();
  private readonly GIST_UUID_EXTRACTION_PATTERN = new RegExp('(?:\/([a-fA-F0-9]+))$')

  public form : FormGroup<{
    url : FormControl<string>
  }>;

  constructor(
    private fb: FormBuilder,
    validator: ValidatorService,
  ) {
    super();
    this.form = this.fb.nonNullable.group({
      url : ['', [Validators.required, Validators.pattern(validator.gistUrlPattern)]]
    });
  }
  
  public onLoadGistUrl(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const gistId = this.extractGistIdFromUrl(this.form.getRawValue().url);
    if (gistId != null){
      this.onGistSelected.emit(gistId);
    }
  }

  private extractGistIdFromUrl(url: string): string | null {
    const match = url.match(this.GIST_UUID_EXTRACTION_PATTERN);
    return match != null
      ? match[0].replace('/','') 
      : null;
  }
}
