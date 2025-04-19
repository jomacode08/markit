import { Routes } from '@angular/router';
import { NotFoundComponent } from './shared/pages/not-found/not-found.component';
import { IsAuthenticatedActivateGuard, IsAuthenticatedMatchGuard } from './auth/guards/auth.guard';

export const routes: Routes = [
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
        path: 'collections',
        data: { breadcrumb : 'Collections' },
        canActivate: [IsAuthenticatedActivateGuard],
        canMatch: [IsAuthenticatedActivateGuard],
        loadComponent: () => 
            import('../app/dashboard/layouts/app-layout.component').then(c => c.AppLayoutComponent),
        loadChildren: () =>
            import('./collections/collections.routes').then(r => r.COLLECTIONS_ROUTES)
    },
    {
        path: 'not-found',
        component: NotFoundComponent
    },
    {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full'
    },
    {
        path: '**',
        redirectTo: 'not-found'
    }
];
