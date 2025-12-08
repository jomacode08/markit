type RedirectState = 'success' | 'failure';
type RedirectPurpose = 'sign-in' | 'link';


export interface RedirectUrlParams {
  state : RedirectState,
  purpose : RedirectPurpose,
  error ?: string;
}

export interface RedirectResponse {
  state: RedirectState,
  purpose : RedirectPurpose,
}