import { Routes } from '@angular/router';

export const DASHBOARD_ROUTES: Routes = [
    {
        path: '',
        pathMatch: 'full',
        data: { breadcrumb : '' },
        loadComponent: () =>
            import('./pages/home/home.component')
            .then(c => c.HomeComponent)
    },
    {
        path: 'profile',
        title: 'My profile',
        data: { breadcrumb : 'Profile' },
        loadComponent: () =>
            import('./pages/profile/profile.component')
            .then(c => c.ProfileComponent)
    }
]