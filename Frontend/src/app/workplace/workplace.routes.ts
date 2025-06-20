import { Routes } from "@angular/router";
import { MAIN_COLLECTION_PARAM } from "../shared/utils/constant";

export const WORKPLACE_ROUTES: Routes = [
    {
        path: '',
        pathMatch: 'full',
        redirectTo: `explore/${ MAIN_COLLECTION_PARAM }`
    },
    {
        path: 'explore/:id',
        loadComponent: () =>
            import('./pages/collection-explorer/collection-explorer.component')
            .then(c => c.CollectionExplorerComponent)
    },
    {
        title: 'Starred',
        path: 'starred',
        loadComponent: () =>
            import('./pages/starred/starred.component')
            .then(c => c.StarredComponent)
    },
    {
        title: 'Recent',
        path: 'recent',
        loadComponent: () =>
            import('./pages/recent/recent.component')
            .then(c => c.RecentComponent)
    },

]