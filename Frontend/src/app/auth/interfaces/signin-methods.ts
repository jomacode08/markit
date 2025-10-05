export interface SignInMethods {
    hasEmail : boolean,
    hasPassword: boolean,
    externalSignMethods: ExternalSignInMethod[]
}

export interface ExternalSignInMethod {
    loginProvider : LoginProvider,
    identifier ?: string,
    configured: boolean,
}

export enum LoginProvider {
    Google = "google"
}

export enum LoginPurpose {
    SignIn = "signIn",
    LinkAccount = "linkAccount"
}