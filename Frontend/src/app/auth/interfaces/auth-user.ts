import { AuthRole } from "./auth-role.enum";

export interface AuthenticatedUser {
    userId: string,
    givenName:   string;
    email: string;
    roles : AuthRole[];
    userPictureUrl ?: string;
}