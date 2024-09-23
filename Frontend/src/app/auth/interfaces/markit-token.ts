export type TokenType = 'base' | 'google';

export interface MarkitToken {
    access_token:  string;
    expires_in:    number;
    origin:    TokenType;
}