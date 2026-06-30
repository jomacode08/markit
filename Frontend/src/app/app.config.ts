import { provideAnimations } from '@angular/platform-browser/animations';
import { APP_INITIALIZER, ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { HTTP_INTERCEPTORS, provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { provideRouter, TitleStrategy, withRouterConfig, withViewTransitions } from '@angular/router';

import { AuthService } from './auth/services/auth.service';
import { HttpRequestInterceptor } from './auth/services/http-request.interceptor';
import { routes } from './app.routes';
import { TemplatePageTitleStrategy } from './shared/utils/template-page-title-strategy';

import { ConfirmationService, MessageService } from 'primeng/api';
import { DialogService } from 'primeng/dynamicdialog';
import { providePrimeNG } from 'primeng/config';
import { CustomPreset } from './shared/styles/theme/custom-preset';
import { THEME } from './shared/utils/constant';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideAnimations(),
    provideRouter(
      routes,
      withRouterConfig({ onSameUrlNavigation: 'reload' }),
      withViewTransitions()
    ),
    provideHttpClient(
      withInterceptorsFromDi()
    ),
    providePrimeNG({
      theme: {
        preset: CustomPreset,
        options: {
          darkModeSelector: `.${THEME.DARK_MODE_SELECTOR}`,
          cssLayer: {
            name: 'primeng',
            order: 'primeng, project',
          },
        }
      }
    }),
    {
      provide: HTTP_INTERCEPTORS,
      useClass: HttpRequestInterceptor,
      multi: true
    },
    {
      provide: TitleStrategy,
      useClass: TemplatePageTitleStrategy
    },
    {
      provide: APP_INITIALIZER,
      useFactory: (authService: AuthService) => () => authService.setupAuthentication(),
      deps: [AuthService],
      multi: true
    },
    MessageService,
    ConfirmationService,
    DialogService,
  ]
};
