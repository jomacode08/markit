import { computed, Injectable, signal } from '@angular/core';
import { GeneralConstant } from '../../shared/utils/general-constant';
import { MarkitToken } from '../interfaces/markit-token';


@Injectable({providedIn: 'root'})
export class AuthService {

    private tokenKey: string = GeneralConstant.token_key;
    
    private getTokenFromLocalStorage(): MarkitToken | null {
        const tokenStr = localStorage.getItem(this.tokenKey);
        return tokenStr ? JSON.parse(tokenStr) as MarkitToken : null;
    }

    public isAuthenticated() {
        const token = this.getTokenFromLocalStorage();
        return token != null && token.expires_in > 0;
    }

    public setToken( token: MarkitToken ): void {
        localStorage.setItem(this.tokenKey, JSON.stringify(token));
    }

    public logout(): void {
        localStorage.removeItem(this.tokenKey);
    }
}