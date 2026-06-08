import { ValidationErrors } from "@angular/forms";

export interface ValidationField {
    name : string,
    normalizedName ?: string;
    validationErrors : ValidationErrors
}