export interface OAuth20Response {
    id_token:  string;
    expires_in:    number;
    token_type:    string;
    scope:         string;
    refresh_token: string;
}
