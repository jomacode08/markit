import { AuthResponse } from "./auth-response";

type RedirectState = 'success' | 'failure';
type RedirectPurpose = 'sign-in' | 'link';


export interface RedirectUrlParams {
  state : RedirectState,
  purpose : RedirectPurpose,
  error ?: string;
  token ?: string;
  meiliToken ?: string;
}

export interface RedirectResponse {
  state: RedirectState,
  auth ?: AuthResponse,
}