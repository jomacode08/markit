export interface AuthenticatedUser {
    userId: string,
    givenName:   string;
    email: string;
    userPictureUrl ?: string;
}