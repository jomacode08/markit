import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Params } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { AuthResponse } from '../../interfaces/auth-response';

export type AuthProcessState = 'idle' | 'authorized' | 'error';

@Component({
  selector: 'app-status',
  standalone: true,
  imports: [
    CommonModule
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './redirect.component.html',
  styleUrl: './redirect.component.css',
})
export class RedirectComponent implements OnInit {
  private activatedRoute = inject(ActivatedRoute);
  
  private readonly AUTH_PARAMS = {
    token : 'token',
    meiliToken : 'meiliToken',
    error: 'error',
  }

  public errorMessage = signal<string>('');
  public currentAuthState = signal<AuthProcessState>('idle');

  public async ngOnInit(): Promise<void> {
    const params = await this.getUrlParams();
    this.handleAuthRedirect(params);
  }

  private handleAuthRedirect(params: Params): void {
    if (!this.isRedirectValid(params))
    return this.handleError(params[this.AUTH_PARAMS.error]);

    this.sendAuthResponse(
      params[this.AUTH_PARAMS.token],
      params[this.AUTH_PARAMS.meiliToken]
    );

    this.setAuthState('authorized');
  }

  private handleError(error: string): void {
    this.errorMessage.set(error);
    this.setAuthState('error');
  }

  private async getUrlParams(): Promise<Params> {
    return firstValueFrom(this.activatedRoute.queryParams);
  }

  private sendAuthResponse(token: string, meiliSearchToken: string): void {
    const authResponse : AuthResponse = {
      token,
      meiliSearchToken
    }
    window.opener.postMessage(authResponse);
  }

  private isRedirectValid(params: Params): boolean {
    const { token, meiliToken, error } = this.AUTH_PARAMS;
    const hasError: boolean = params[error] != null;
    const hasValidTokens: boolean = [params[token], params[meiliToken]]
      .every((token : string | null) => token != null && token.trim().length > 0);

    return hasValidTokens && !hasError;
  }

  private setAuthState = (status: AuthProcessState) => this.currentAuthState.update(() => status);
}
