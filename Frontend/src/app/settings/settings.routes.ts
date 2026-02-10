import { Routes } from "@angular/router";
import { AuthRole } from "../auth/interfaces/auth-role.enum";
import { hasRoleActivateGuard, hasRoleMatchGuard } from "../auth/guards/has-role/has-role.guard";

export const SETTINGS_ROUTES: Routes = [
    {
        path: '',
        pathMatch: 'full',
        redirectTo: '/dashboard'
    },
    {
        path: 'accounts',
        title: 'Accounts',
        canActivate: [hasRoleActivateGuard([AuthRole.ADMIN])],
        canMatch: [hasRoleMatchGuard([AuthRole.ADMIN])],
        loadComponent: () =>
            import('./pages/account-list/account-list.component')
            .then(c => c.AccountListComponent)
    }
];