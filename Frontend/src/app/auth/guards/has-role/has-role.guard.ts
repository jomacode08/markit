import { CanActivateFn, CanMatchFn, Router } from "@angular/router";
import { inject } from "@angular/core";
import { AuthRole } from "../../interfaces/auth-role.enum";
import { AuthService } from "../../services/auth.service";

//* === Utilities === *//
/**
 * Checks if the current user has at least one of the specified roles.
 * @param expectedRoles Array of roles required for access.
 * @returns True if the user has all expected roles, false otherwise.
 */
const RequireRole = (expectedRoles: AuthRole[]): boolean => {
    const authService = inject(AuthService);
    const userRoles : AuthRole[] | undefined = authService.currentUser()?.roles;
    if (!userRoles) return false;
    return expectedRoles.some(r => userRoles.includes(r));
}

/**
 * Checks user authorization and navigates to '/unauthorized' if not authorized.
 * @param expectedRoles Array of roles required for access.
 * @returns True if the user is authorized, false otherwise.
 */
const checkUserAuthorization = (expectedRoles: AuthRole[]): boolean => {
    const hasRoles: boolean = RequireRole(expectedRoles);
    if (!hasRoles) {
        const router = inject(Router);
        router.navigate(['/unauthorized']);
    }
    return hasRoles;
}

//* === Guard Methods === *//

/**
 * Guard for route activation based on user roles.
 * @param roles Array of roles required for route activation.
 * @returns CanActivateFn that checks user authorization.
 */
export const hasRoleActivateGuard = (roles: AuthRole[]): CanActivateFn => {
    return () => checkUserAuthorization(roles);
}

/**
 * Guard for route matching based on user roles.
 * @param roles Array of roles required for route matching.
 * @returns CanMatchFn that checks user authorization.
 */
export const hasRoleMatchGuard = (roles: AuthRole[]): CanMatchFn => {
    return () => checkUserAuthorization(roles);
}