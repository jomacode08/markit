import { Routes } from "@angular/router";

export const COLLECTIONS_ROUTES: Routes = [
    {
        path: '',
        pathMatch: 'full',
        redirectTo: 'root'
    },
    {
        path: ':id',
        loadComponent: () =>
            import('./pages/collection-explorer/collection-explorer.component')
            .then(c => c.CollectionExplorerComponent)
    },
]