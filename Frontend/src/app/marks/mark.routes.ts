import { Routes } from "@angular/router";

export const MARK_ROUTES: Routes = [
    {
        path: '',
        pathMatch: 'full',
        title: 'My Marks',
        data: { breadcrumb : '' },
        loadComponent: () =>
            import('./pages/mark-list/mark-list.component')
            .then(c => c.MarkListComponent)
    },
    {
        path: 'new',
        title: '',
        data: { breadcrumb : 'New' },
        loadComponent: () =>
            import('./pages/mark-viewer/mark-viewer.component')
            .then(c => c.MarkViewerComponent)
    },
    {
        path: 'see/:id',
        title: '',
        data: { breadcrumb : 'See' },
        loadComponent: () =>
            import('./pages/mark-viewer/mark-viewer.component')
            .then(c => c.MarkViewerComponent)
    },
];