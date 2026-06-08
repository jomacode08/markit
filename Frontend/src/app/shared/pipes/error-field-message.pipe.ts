import { Pipe, PipeTransform } from '@angular/core';
import { ValidationField } from '../interfaces/form/validation-field.interface';

@Pipe({
    standalone: true,
    name: 'errorFieldMessage'
})

export class ErrorFieldMessagePipe implements PipeTransform {
    transform( field: ValidationField | null ): string {
        if (!field) return '';

        let name : string    = field.normalizedName ?? field.name;
        // Obtener Keys contenidas en ValidationError
        const errors = Object.keys(field.validationErrors);

        // Retornar el mensaje de error del primer error encontrado.
        switch (errors[0]) {
            case 'required':
                return `The ${ name } is required`;
            case 'minlength':
                return `The ${ name } field must be at least ${ field.validationErrors!['minlength'].requiredLength } characters`
            case 'maxlength':
                return `The ${ name } field cannot exceed ${ field.validationErrors!['maxlength'].requiredLength } characters`
            case 'pattern' :
                return `The ${ name } field has an invalid format`;
            case 'notEqual' :
                return `The value of ${ name.toLowerCase() } is incorrect`;
            case 'notGroupValidCheckbox' :
                return `At least one must be selected`;
            default:
                return '';
        }
    }
}