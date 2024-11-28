import { Routes } from '@angular/router';

export const DASHBOARD_ROUTES: Routes = [
    {
        path: '',
        loadComponent: () => import('./layouts/app-layout.component').then(c => c.AppLayoutComponent),
        children: [
            {
                path: '',
                pathMatch: 'full',
                redirectTo: 'home'
            },
            {
                path: 'home',
                title: 'Home',
                loadComponent: () =>
                    import('./pages/home/home.component')
                    .then(c => c.HomeComponent)
            },
            {
                path: 'profile',
                title: 'Profile',
                loadComponent: () =>
                    import('./pages/profile/profile.component')
                    .then(c => c.ProfileComponent)
            },
            {
                path: 'group',
                title: 'Group',
                loadComponent: () =>
                    import('./pages/group/group.component')
                    .then(c => c.GroupComponent)
            },
        ]
    },
]