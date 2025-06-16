import { Routes } from "@angular/router";
import { MAIN_COLLECTION_PARAM } from "../shared/utils/constant";

export const WORKPLACE_ROUTES: Routes = [
    {
        path: '',
        pathMatch: 'full',
        redirectTo: MAIN_COLLECTION_PARAM
    },
    {
        path: ':id',
        loadComponent: () =>
            import('./pages/collection-explorer/collection-explorer.component')
            .then(c => c.CollectionExplorerComponent)
    }
]