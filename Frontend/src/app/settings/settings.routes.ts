import { Routes } from "@angular/router";
import { AuthRole } from "../auth/interfaces/auth-role.enum";
import { hasRoleActivateGuard, hasRoleMatchGuard } from "../auth/guards/has-role/has-role.guard";
import { SettingsLayoutComponent } from "./layout/settings-layout.component";

export const SETTINGS_ROUTES: Routes = [
    {
        path: '',
        component: SettingsLayoutComponent,        
        children: [
            {
                path: '',
                pathMatch: 'full',
                redirectTo: 'accounts'
            },
            {
                path: 'accounts',
                title: `Accounts`,
                canActivate: [hasRoleActivateGuard([AuthRole.ADMIN])],
                canMatch: [hasRoleMatchGuard([AuthRole.ADMIN])],
                loadComponent: () =>
                    import('./pages/account-list/account-list.component')
                    .then(c => c.AccountListComponent)
            },        
            {
                path: 'demo',
                title: `Demo`,
                canActivate: [hasRoleActivateGuard([AuthRole.ADMIN])],
                canMatch: [hasRoleMatchGuard([AuthRole.ADMIN])],
                loadComponent: () =>
                    import('./pages/demo-form/demo-form.component')
                    .then(c => c.DemoFormComponent)
            }        
        ]
    },
];