import { ChangeDetectionStrategy, Component, DestroyRef, OnInit, signal } from '@angular/core';
import { finalize } from 'rxjs';
import { FormBuilder, FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';

import { ButtonModule } from 'primeng/button';
import { ToggleSwitchModule } from 'primeng/toggleswitch';

import { CustomMessageService } from '../../../shared/services/custom-message.service';
import { ExternalAuthSettings } from '../../interfaces/external-auth-settings';
import { ExternalLoginService } from '../../../auth/services/external-login.service';
import { ROUTES } from '../../../shared/utils/constant';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

@Component({
  selector: 'settings-authentication',
  imports: [
    ButtonModule,
    ReactiveFormsModule,
    ToggleSwitchModule,
  ],
  templateUrl: './authentication-settings.component.html',
  styleUrl: './authentication-settings.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AuthenticationSettingsComponent implements OnInit {
  private readonly CONFIRMATION_MESSAGE : string = "The settings were updated successfully.";
  public form : FormGroup<{
    isGoogleEnabled : FormControl<boolean>,
    isGitHubEnabled : FormControl<boolean>
  }>;
  protected isSubmitting = signal<boolean>(false);

  constructor(
    private fb : FormBuilder,
    private messageService: CustomMessageService,
    private externalLoginService: ExternalLoginService,
    private router: Router,
    private destroyRef: DestroyRef
  ){
    this.form = fb.nonNullable.group({
      isGoogleEnabled: [false],
      isGitHubEnabled : [false],
    });
  }

  public ngOnInit(): void {
    this.externalLoginService.getSettings()
    .pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (settings) => this.form.reset(settings),
      error: () => this.router.navigate([ROUTES.ERROR])
    });
  }

  public onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    const settings : ExternalAuthSettings = this.form.getRawValue();
    this.updateSettings(settings);
  }

  private updateSettings(settings: ExternalAuthSettings): void {
    this.externalLoginService.updateSettings(settings)
    .pipe(finalize(() => this.isSubmitting.set(false)))
    .subscribe(() => this.messageService.showGeneralSuccess(this.CONFIRMATION_MESSAGE));
  }
}