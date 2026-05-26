export interface Account {
    userId: string;
    name: string;
    userName: string;
    roles: string[];
    enabled: boolean;
    picture?: string;
}

export interface PasswordRequest {
    currentPassword: string;
    newPassword: string;
}