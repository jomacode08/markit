import { Pipe, PipeTransform } from '@angular/core';
import { ValidationField } from '../interfaces/form/form/validation-field.interface';

@Pipe({
    name: 'errorFieldMessage'
})

export class ErrorFieldMessagePipe implements PipeTransform {
    transform( field: ValidationField | null ): string {
        if (!field) return '';

        // Gestión de pronombre gramatical
        let pronoun: string = '';
        let endsWith: string = '';
        let name : string = field.normalizedName ?? field.name;

        switch (field.gender) {
            case 'fem':
                pronoun = 'La';
                endsWith = 'a';
                break;

            case 'masc':
                pronoun = 'El';
                endsWith = 'o';
                break;

            default:
                break;
        }

        // Obtener Keys contenidas en ValidationError
        const errors = Object.keys(field.validationError);

        // Retornar el mensaje de error del primer error encontrado.
        switch (errors[0]) {
            case 'required':
                return `${ pronoun } ${ name } es requerid${ endsWith }`;
            case 'minlength':
                return `Se requieren mínimo ${ field.validationError!['minlength'].requiredLength } caracteres`
            case 'maxlength':
                return `El límite máximo es de ${ field.validationError!['maxlength'].requiredLength } caracteres`
            case 'pattern' :
                return `${ pronoun } ${ name } cuenta con un formato inválido`;
            case 'notEqual' :
                return `${ pronoun } ${ name } no es correct${ endsWith }`;
            case 'notGroupValidCheckbox' :
                return `Por lo menos un campo debe ser seleccionado`;
            default:
                return '';
        }

    }
}