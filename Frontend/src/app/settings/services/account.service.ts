import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';
import { Account, PasswordRequest } from '../interfaces/account';

@Injectable({providedIn: 'root'})
export class AccountService {
    private readonly BASE_URL: string = `${ environment.baseApiUrl }/accounts`;
    
    constructor(private http: HttpClient) { }

    public getByCurrentSession(): Observable<Account> {
        return this.http.get<Account>(`${this.BASE_URL}/me`);
    }
    public getByUserId(userId: string): Observable<Account> {
        return this.http.get<Account>(`${this.BASE_URL}/${userId}`);
    }
    public create(account: Account, password: PasswordRequest): Observable<Account> {
        return this.http.post<Account>(`${this.BASE_URL}`, { account, password });
    }
    public update(account: Account): Observable<Account> {
        return this.http.put<Account>(`${this.BASE_URL}/${account.userId}`, account);
    }
    public updateMyProfile(name: string, userName: string): Observable<Account> {
        return this.http.put<Account>(`${this.BASE_URL}/me/profile`, {
            name,
            userName
        });
    }
}