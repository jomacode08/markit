import { Routes } from '@angular/router';
import { IsAuthenticatedActivateGuard, IsAuthenticatedMatchGuard } from './auth/guards/auth.guard';

export const routes: Routes = [
    {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full'
    },
    {
        path: 'auth',
        loadComponent: () => 
            import('../app/auth/layout/auth-layout.component').then(c => c.AuthLayoutComponent),
        loadChildren: () =>
            import('./auth/auth.routes').then(r => r.AUTH_ROUTES)
    },
    {
        path: 'dashboard',
        data: { breadcrumb : 'Dashboard' },
        canActivate: [IsAuthenticatedActivateGuard],
        canMatch: [IsAuthenticatedMatchGuard],
        loadComponent: () => 
            import('../app/dashboard/layouts/app-layout.component').then(c => c.AppLayoutComponent),
        loadChildren: () =>
            import('./dashboard/dashboard.routes').then(r => r.DASHBOARD_ROUTES)
    },
    {
        path: 'marks',
        data: { breadcrumb : 'Marks' },
        canActivate: [IsAuthenticatedActivateGuard],
        canMatch: [IsAuthenticatedActivateGuard],
        loadComponent: () => 
            import('../app/dashboard/layouts/app-layout.component').then(c => c.AppLayoutComponent),
        loadChildren: () =>
            import('./marks/mark.routes').then(r => r.MARK_ROUTES)
    },
    {
        path: 'settings',
        data: { breadcrumb : 'Settings' },
        canActivate: [IsAuthenticatedActivateGuard],
        canMatch: [IsAuthenticatedActivateGuard],
        loadComponent: () => 
            import('../app/dashboard/layouts/app-layout.component').then(c => c.AppLayoutComponent),
        loadChildren: () =>
            import('./settings/settings.routes').then(r => r.SETTINGS_ROUTES)
    },
    {
        path: 'workplace',
        data: { breadcrumb : 'Workplace' },
        canActivate: [IsAuthenticatedActivateGuard],
        canMatch: [IsAuthenticatedActivateGuard],
        loadComponent: () => 
            import('../app/dashboard/layouts/app-layout.component').then(c => c.AppLayoutComponent),
        loadChildren: () =>
            import('./workplace/workplace.routes').then(r => r.WORKPLACE_ROUTES)
    },
    {
        path: 'error',
        loadComponent: () => 
            import('../app/shared/pages/error/error.component')
            .then(c => c.ErrorComponent)
    },
    {
        path: 'not-found',
        loadComponent: () => 
            import('../app/shared/pages/not-found/not-found.component')
            .then(c => c.NotFoundComponent)
    },
    {
        path: 'unauthorized',
        loadComponent: () => 
            import('../app/shared/pages/unauthorized/unauthorized.component')
            .then(c => c.UnauthorizedComponent)
    },
    {
        path: '**',
        redirectTo: 'not-found'
    },
];
