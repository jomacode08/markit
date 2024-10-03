import { ValidationError } from "../../../auth/interfaces/error-exception";

export interface CustomMessage {
    type : MessageType,
    title : string,
    icon ?: string,
    message ?: string,
    validations ?: ValidationError
}

export enum MessageType {
    success = 'success',
    info = 'info',
    warn = 'warn',
    error = 'error'
}