import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, OnInit, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { RedirectResponse, RedirectUrlParams } from '../../interfaces/redirect';
import { AuthResponse } from '../../interfaces/auth-response';
import { PopupService } from '../../../shared/services/popup.service';

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
  public errorMessage = signal<string>('');
  public currentAuthState = signal<AuthProcessState>('idle');

  constructor(
    private poupService: PopupService,
    private activatedRoute : ActivatedRoute,
  ) {}

  public async ngOnInit(): Promise<void> {
    const redirectParams: RedirectUrlParams = await this.getUrlParams();
    this.handleRedirect(redirectParams);
  }

  private handleRedirect(params: RedirectUrlParams): void {
    if (!this.isRedirectValid(params))
      return this.handleError(params.error ?? 'External login failed.');

    this.sendRedirectResponse(params);
    this.setAuthState('authorized');
  }

  private handleError(error: string): void {
    this.errorMessage.set(error);
    this.setAuthState('error');
  }

  private async getUrlParams(): Promise<RedirectUrlParams> {
    const params = await firstValueFrom(this.activatedRoute.queryParams);
    return params as RedirectUrlParams;
  }

  private sendRedirectResponse(params: RedirectUrlParams): void {
    const { state, purpose, token, meiliToken } = params;
    let auth : AuthResponse | undefined = undefined;

    if (purpose === 'sign-in') {
      auth = { token, meiliSearchToken: meiliToken } as AuthResponse;
    }

    this.poupService.sendMessageToOpener({
      state,
      auth
    } as RedirectResponse);
  }

  private isRedirectValid(params: RedirectUrlParams): boolean {
    const { state, purpose, token, meiliToken, error } = params;
    if (state === 'failure' || error != undefined) return false;

    if (purpose === 'sign-in') {
      const hasValidTokens: boolean = [token, meiliToken]
        .every((token : string | undefined) => token != undefined && token.trim().length > 0);
      return hasValidTokens;
    }

    return true;
  }

  private setAuthState = (status: AuthProcessState) => this.currentAuthState.update(() => status);
}