import { FormGroup } from "@angular/forms";
import { GramaticalGender } from "../interfaces/form/gramatical-gender.type";
import { ValidationField } from "../interfaces/form/validation-field.interface";

export abstract class ValidatorErrorField {

    // Propiedad que representa el formulario y deberá de ser declarada en el componente hijo.
    public abstract form : FormGroup;

    public isInvalidField( fieldName: string ): boolean | null {
        const formControl = this.form.controls[fieldName];
        if (!formControl) return null;

        return formControl.errors && formControl.touched;
    }
    
    /** 
     * Método que permite mapear el objeto @see ValidationField que es utilizado en el componente @see ErrorFieldComponent
    **/
    public getValidationField( fieldName: string, gender : GramaticalGender, normalizedName ?: string ): ValidationField | null {
        // Validación de existencia del formControl y que cuente con errores para continuar
        const formControl = this.form.controls[fieldName];
        if (!formControl || !formControl.errors) return null;

        // Variable de tipo ValidationErrors, que contiene los errores del control mediante keys
        const validationErrors = formControl.errors ?? {};

        // Marcar el campo como Dirty
        formControl.markAsDirty();

        // Retornar el objeto CustomField
        return {
            name : fieldName,
            normalizedName : normalizedName,
            gender : gender,
            validationError : validationErrors
        };

    }
}