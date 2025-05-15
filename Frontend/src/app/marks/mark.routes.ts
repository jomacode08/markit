import { Routes } from "@angular/router";
import { canDeactivateGuard } from "../auth/guards/can-deactivate/can-deactivate.guard";

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
        canDeactivate: [canDeactivateGuard],
        loadComponent: () =>
            import('./pages/mark-viewer/mark-viewer.component')
        .then(c => c.MarkViewerComponent)
    },
    {
        path: 'see/:id',
        title: '',
        data: { breadcrumb : 'See' },
        canDeactivate: [canDeactivateGuard],
        loadComponent: () =>
            import('./pages/mark-viewer/mark-viewer.component')
            .then(c => c.MarkViewerComponent)
    },
];