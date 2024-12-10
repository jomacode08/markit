import { Pipe, PipeTransform } from '@angular/core';
import { ValidationField } from '../interfaces/form/validation-field.interface';

@Pipe({
    name: 'errorFieldMessage'
})

export class ErrorFieldMessagePipe implements PipeTransform {
    transform( field: ValidationField | null ): string {
        if (!field) return '';

        let name : string    = field.normalizedName ?? field.name;
        // Obtener Keys contenidas en ValidationError
        const errors = Object.keys(field.validationError);

        // Retornar el mensaje de error del primer error encontrado.
        switch (errors[0]) {
            case 'required':
                return `The ${ name } field is required`;
            case 'minlength':
                return `A minimum of ${ field.validationError!['minlength'].requiredLength } characters is required`
            case 'maxlength':
                return `A maximum of ${ field.validationError!['maxlength'].requiredLength } characters is required`
            case 'pattern' :
                return `The ${ name } field has an invalid format`;
            case 'notEqual' :
                return `The value of ${ name.toLowerCase() } isn't correct`;
            case 'notGroupValidCheckbox' :
                return `At least one field must be selected`;
            default:
                return '';
        }

    }
}