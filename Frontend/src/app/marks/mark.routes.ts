import { Routes } from "@angular/router";

export const MARK_ROUTES: Routes = [
    {
        path: '',
        loadComponent: () => import('../dashboard/layouts/app-layout.component').then(c => c.AppLayoutComponent),
        children: [
            {
                path: '',
                pathMatch: 'full',
                redirectTo: 'list'
            },
            {
                path: 'list',
                title: 'My marks',
                loadComponent: () =>
                    import('./pages/mark-list/mark-list.component')
                    .then(c => c.MarkListComponent)
            },
            {
                path: 'new',
                title: '',
                loadComponent: () =>
                    import('./pages/mark-viewer/mark-viewer.component')
                    .then(c => c.MarkViewerComponent)
            },
            {
                path: 'see/:id',
                title: '',
                loadComponent: () =>
                    import('./pages/mark-viewer/mark-viewer.component')
                    .then(c => c.MarkViewerComponent)
            },
        ]
    }
];