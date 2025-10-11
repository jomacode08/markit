export interface SignInMethods {
    hasEmail : boolean,
    hasPassword: boolean,
    externalSignMethods: ExternalSignInMethod[]
}

export interface ExternalSignInMethod {
    loginProvider : LoginProvider,
    providerName : string,
    identifier ?: string,
    configured: boolean,
}

export enum LoginProvider {
    Google = "google",
    GitHub = "gitHub"
}

export enum LoginPurpose {
    SignIn = "signIn",
    LinkAccount = "linkAccount"
}