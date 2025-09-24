export interface SignInMethods {
    hasEmail : boolean,
    hasPassword: boolean,
    externalSignMethods: ExternalSignInMethod[]
}

export interface ExternalSignInMethod {
    loginProvider : LoginProvider,
    identifier: string,
}

export enum LoginProvider {
    Google = "google"
}