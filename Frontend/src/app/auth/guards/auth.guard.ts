import { inject } from "@angular/core";
import { ActivatedRouteSnapshot, CanActivateFn, CanMatchFn, Route, Router, RouterStateSnapshot, UrlSegment } from "@angular/router";
import { AuthService } from "../services/auth.service";

const checkAuthStatus = (): boolean => 
{
    const authService = inject(AuthService);
    return authService.isAuthenticated();
}

const isAuthenticated = (): boolean => 
{
    const router = inject(Router);

    if (!checkAuthStatus()) {
        router.navigate(['auth']);
    }

    return checkAuthStatus();
}

// CanActivate GUARDS
export const IsAuthenticatedActivateGuard : CanActivateFn = (
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
): boolean => 
{
    return isAuthenticated();
}

export const IsNotAuthenticatedActivateGuard : CanActivateFn = (
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
): boolean => 
{
    const router = inject(Router);
    const isAuthenticaded = checkAuthStatus();
    
    if (isAuthenticaded) router.navigate(['/']);
    return !isAuthenticaded;
}

// Can Match Guards
export const IsAuthenticatedMatchGuard: CanMatchFn = (
    route: Route,
    segments: UrlSegment[]
): boolean => 
{
    return isAuthenticated();
}