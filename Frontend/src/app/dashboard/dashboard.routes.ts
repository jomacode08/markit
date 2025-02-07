import { Routes } from '@angular/router';

export const DASHBOARD_ROUTES: Routes = [
    {
        path: '',
        pathMatch: 'full',
        title: 'Home',
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
    },
    {
        path: 'group',
        title: 'My groups',
        data: { breadcrumb : 'Groups' },
        loadComponent: () =>
            import('./pages/group/group.component')
            .then(c => c.GroupComponent)
    },
]