import { Routes } from "@angular/router";
import { canDeactivateGuard } from "../auth/guards/can-deactivate/can-deactivate.guard";

export const NOTEBOOK_ROUTES: Routes = [
    {
        path: '',
        pathMatch: 'full',
        redirectTo: '/collections'
    },
    {
        path: 'see/:id',
        data: { breadcrumb : 'See' },
        canDeactivate: [canDeactivateGuard],
        loadComponent: () =>
            import('./pages/notebook-viewer/notebook-viewer.component')
            .then(c => c.NotebookViewerComponent)
    },
];