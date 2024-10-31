import { IsAuthenticatedActivateGuard, IsNotAuthenticatedActivateGuard } from './guards/auth.guard';
import { Routes } from '@angular/router';

export const AUTH_ROUTES: Routes = [
    {
        path: '',
        loadComponent: () => import('./layout/auth-layout.component').then(c => c.AuthLayoutComponent),
        children: [
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
                path: 'status',
                canActivate: [IsNotAuthenticatedActivateGuard],
                loadComponent: () =>
                    import('./pages/loader/loader.component')
                    .then(c => c.LoaderComponent)
            }
        ]
    }
]
