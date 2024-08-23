import { Routes } from '@angular/router';

export const routes: Routes = [
    {
        path: 'auth',
        children: [
            {
                path: 'login',
                loadComponent: () => 
                    import('./auth/pages/login/login.component')
                    .then(c => c.LoginComponent)
            },
            {
                path: '',
                pathMatch: 'full',
                redirectTo: 'login'
            }
        ]
    },
    {
        path: 'dashboard',
        loadComponent: () => import('./dashboard/layouts/app-layout.component').then(c => c.AppLayoutComponent),
        children: [
            {
                path: 'home',
                loadComponent: () =>
                    import('./dashboard/pages/home/home.component')
                    .then(c => c.HomeComponent)
            },
            {
                path: 'group',
                loadComponent: () =>
                    import('./dashboard/pages/group/group.component')
                    .then(c => c.GroupComponent)
            },
            {
                path: '',
                pathMatch: 'full',
                redirectTo: 'home'
            }
        ]
    },
    {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full'
    },
];
