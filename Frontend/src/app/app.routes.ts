import { Routes } from '@angular/router';
import { NotFoundComponent } from './shared/pages/not-found/not-found.component';
import { IsAuthenticatedActivateGuard, IsAuthenticatedMatchGuard } from './auth/guards/auth.guard';

export const routes: Routes = [
    {
        path: 'auth',
        loadChildren: () =>
            import('./auth/auth.routes').then(r => r.AUTH_ROUTES)
    },
    {
        path: 'dashboard',
        canActivate: [IsAuthenticatedActivateGuard],
        canMatch: [IsAuthenticatedMatchGuard],
        loadChildren: () =>
            import('./dashboard/dashboard.routes').then(r => r.DASHBOARD_ROUTES)
    },
    {
        path: 'mark',
        canActivate: [IsAuthenticatedActivateGuard],
        canMatch: [IsAuthenticatedActivateGuard],
        loadChildren: () => 
            import('./marks/mark.routes').then(r => r.MARK_ROUTES)
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
