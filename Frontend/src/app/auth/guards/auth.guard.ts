import { inject } from "@angular/core";
import { ActivatedRouteSnapshot, CanActivateFn, CanMatchFn, Route, Router, RouterStateSnapshot, UrlSegment } from "@angular/router";
import { AuthService } from "../services/auth.service";
import { AuthStatus } from "../interfaces/auth-status.enum";

//* === Utilities === *//
const checkAuthenticationStatus = (): boolean => 
{
    const authService = inject(AuthService);
    return authService.authStatus() === AuthStatus.authenticated;
}

const requireAuthentication = (): boolean => 
{
    const isAuthenticated : boolean = checkAuthenticationStatus();
    
    if (!isAuthenticated) {
        const router = inject(Router);
        router.navigate(['auth/login']);
    }
    
    return isAuthenticated;
}

//* === Guard Methods === *//
//* For Routes that requires authentication
export const IsAuthenticatedActivateGuard : CanActivateFn = (
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
): boolean => 
{
    return requireAuthentication();
}

export const IsAuthenticatedMatchGuard: CanMatchFn = (
    route: Route,
    segments: UrlSegment[]
): boolean => 
{
    return requireAuthentication();
}


//* For routes that requires no-authentication behaviour
export const IsNotAuthenticatedActivateGuard : CanActivateFn = (
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
): boolean => 
{
    const router = inject(Router);
    const isAuthenticaded = checkAuthenticationStatus();
    
    if (isAuthenticaded) router.navigate(['/']);
    return !isAuthenticaded;
}
