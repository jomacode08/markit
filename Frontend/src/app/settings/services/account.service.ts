import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';
import { Account } from '../interfaces/account';

@Injectable({providedIn: 'root'})
export class AccountService {
    private readonly BASE_URL: string = `${ environment.baseApiUrl }/accounts`;
    
    constructor(private http: HttpClient) { }

    public getByUserId( userId: string ): Observable<Account> {
        return this.http.get<Account>(`${ this.BASE_URL }/${userId}`);
    }
    public create( account: Account ): Observable<Account> {
        return this.http.post<Account>(`${ this.BASE_URL }`, account);
    }
    public update( account: Account ): Observable<Account> {
        return this.http.put<Account>(`${ this.BASE_URL }/${account.userId}`, account);
    }
}