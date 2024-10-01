import { Injectable } from '@angular/core';
import { GeneralConstant } from '../../shared/utils/general-constant';


@Injectable({providedIn: 'root'})
export class AuthService {

    private tokenKey: string = GeneralConstant.token_key;
    
    private getTokenFromLocalStorage(): string | null {
        return localStorage.getItem(this.tokenKey);
    }

    public isAuthenticated() {
        // TODO: Implementing validation time of the token 
        const token = this.getTokenFromLocalStorage();
        return token != null;
    }

    public setToken( token: string ): void {
        localStorage.setItem(this.tokenKey, token);
    }

    public logout(): void {
        localStorage.removeItem(this.tokenKey);
    }
}