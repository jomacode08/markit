import { ValidationErrors } from "@angular/forms";
import { GramaticalGender } from "./gramatical-gender.type";

export interface ValidationField {
    name : string,
    normalizedName ?: string;
    gender : GramaticalGender,
    validationError : ValidationErrors
}