export interface Account {
    userId:    string;
    creatorId: number;
    firstName: string;
    lastName:  string;
    userName:  string;
    roles:     string[];
    enabled : boolean;
}

export interface PasswordRequest {
    currentPassword: string;
    newPassword: string;
}