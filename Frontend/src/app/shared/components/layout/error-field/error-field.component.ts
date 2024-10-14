import { Component, Input } from '@angular/core';
import { ValidationField } from '../../../interfaces/form/validation-field.interface';

@Component({
  selector: 'shared-error-field',
  templateUrl: './error-field.component.html'
})
export class ErrorFieldComponent {
  
  @Input()
  public validationField : ValidationField | null = null;

}
