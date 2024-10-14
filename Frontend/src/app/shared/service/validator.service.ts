import { Injectable } from '@angular/core';
import { AbstractControl, ValidationErrors } from '@angular/forms';

@Injectable({ providedIn: 'root' })
export class ValidatorService {
    //#region Patterns
    public rfcPattern : string = '^[A-Z&Ñ]{3,4}[0-9]{2}(0[1-9]|1[012])(0[1-9]|[12][0-9]|3[01])[A-Z0-9]{2}[0-9A]$';
    public emailPattern : string = '^[a-z0-9._%+-]+@[a-z0-9.-]+\\.[a-z]{2,4}$';
    public passwordPattern : string = '^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9]).{8,}$';
    //#endregion

    /**
     * Permite validar que dos campos sean iguales.
     * @param field1 primer campo a comparar.
     * @param field2 segundo campo a comparar.
     * @returns devuelve y asigna al segundo campo un ValidationErrors con key 'notEqual' si la validación no se cumple. 
     */
    public isTwoFieldsEquals( field1:string, field2:string ) : ValidationErrors | null {
        return ( form : AbstractControl<any, any> ) => {
            
            // Obtener valores de los dos campos a comparar
            const fieldValue1 = form.get(field1)?.value;
            const fieldValue2 = form.get(field2)?.value;

            // Obtener el campo al que se le asignará el error en caso de que ocurra.
            const evaluatedControl = form.get(field2);
            const evaluatedControlErrors = evaluatedControl?.errors;

            // Comparar los valores de los campos
            if ( fieldValue1 != fieldValue2 ) {
                // Asignar el error al segundo campo
                evaluatedControl?.setErrors({...evaluatedControlErrors, notEqual : true })
                return { notEqual : true }
            }
            
            // Eliminar el error del campo si este existe.
            delete evaluatedControl?.errors?.['notEqual'];
            return null;
        }
    }

    /**
     * Permite validar en un par de campos check-box, que al menos uno se encuentre activado.
     * @param field1 primer campo a comparar.
     * @param field2 segundo campo a comparar.
     * @returns devuelve y asigna al primer campo un ValidationErrors con key 'notGroupValidCheckbox' si la validación no se cumple. 
     */
    public AtLeastOneCheckboxActivated( field1:string, field2:string ) : ValidationErrors | null {
        return ( form : AbstractControl<any, any> ) => {
            
            // Obtener valores de los dos campos a comparar
            const fieldValue1 = form.get(field1)?.value;
            const fieldValue2 = form.get(field2)?.value;

            // Obtener el campo al que se le asignará el error en caso de que ocurra.
            const evaluatedControl = form.get(field1);
            const evaluatedControlErrors = evaluatedControl?.errors;

            // Comparar los valores de los campos
            if ( fieldValue1 === false && fieldValue2 === false) {
                // Asignar el error al primer campo
                evaluatedControl?.setErrors({...evaluatedControlErrors, notGroupValidCheckbox : true })
                return { notGroupValidCheckbox : true }
            }
            
            // Eliminar el error del campo si este existe.
            if (evaluatedControl?.errors?.['notGroupValidCheckbox']) {
                delete evaluatedControl?.errors?.['notGroupValidCheckbox'];
                evaluatedControl?.updateValueAndValidity();
            }
            
            return null;
        }
    }

}