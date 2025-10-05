import { IsNotAuthenticatedActivateGuard } from './guards/auth.guard';
import { Routes } from '@angular/router';

export const AUTH_ROUTES: Routes = [
    {
        path: '',
        pathMatch: 'full',
        redirectTo: 'login'
    },
    {
        path: 'login',
        canActivate: [IsNotAuthenticatedActivateGuard],
        loadComponent: () => 
            import('./pages/login/login.component')
            .then(c => c.LoginComponent)
    },
    {
        path: 'redirect',
        loadComponent: () =>
            import('./pages/redirect/redirect.component')
            .then(c => c.RedirectComponent)
    }
]
