import { Component, Input } from '@angular/core';
import { ValidationField } from '../../../interfaces/form/validation-field.interface';
import { ErrorFieldMessagePipe } from '../../../pipes/error-field-message.pipe';

@Component({
  selector: 'shared-error-field',
  standalone: true,
  imports: [ErrorFieldMessagePipe],
  templateUrl: './error-field.component.html'
})
export class ErrorFieldComponent {
  
  @Input()
  public validationField : ValidationField | null = null;

}
