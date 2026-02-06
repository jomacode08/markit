import { computed, Injectable, signal } from '@angular/core';
import { catchError, Observable, of, Subject, switchMap, tap } from 'rxjs';
import { HttpClient } from '@angular/common/http';

import { AccountPagination } from '../interfaces/account-pagination';
import { AccountSummary } from '../interfaces/account-summary';
import { environment } from '../../../environments/environment';

export enum AccountPaginationStatus {
    idle,
    fetching,
    finished,
    error
}

/**
 * Service for managing paginated account data.
 *
 * This service handles fetching and caching account summaries with pagination support.
 * It uses Angular signals for reactive state management and RxJS for asynchronous operations.
 */
@Injectable({providedIn: 'root'})

export class AccountPaginationService {
    private readonly BASE_URL: string = `${ environment.baseApiUrl }/accounts`;
    private readonly PAGE_SIZE: number = 10;
    private _accounts = signal<AccountSummary[]>([]);
    private _totalPages = signal<number>(1);
    private _totalItems = signal<number>(0);
    private _status = signal<AccountPaginationStatus>(AccountPaginationStatus.idle);
    private pageRequest$ = new Subject<number>();
    //! To the external world
    public accounts = computed<AccountSummary[]>(() => this._accounts());
    public totalItems = computed<number>(() => this._totalItems());
    public totalPages = computed<number>(() => this._totalPages());
    public status = computed<AccountPaginationStatus>(() => this._status());

    constructor(private http: HttpClient) {
        this.pageRequest$.pipe(
            tap(() => this._status.set(AccountPaginationStatus.fetching)),
            switchMap((pageIndex: number) => this.fetchAccountPagination(pageIndex))
        ).subscribe((pagination : AccountPagination | null) => {
            if (pagination != null) {
                this._accounts.set(pagination.data);
                this._totalItems.set(pagination.meta.totalItems);
                this._totalPages.set(pagination.meta.totalPages);
                this._status.set(AccountPaginationStatus.finished);
            } 
        });
    }

    public loadPage(pageIndex : number) {
        this.pageRequest$.next(pageIndex);
    }

    private fetchAccountPagination(pageIndex: number) : Observable<AccountPagination | null> {
        const url = `${ this.BASE_URL }/all?page=${ pageIndex }&limit=${ this.PAGE_SIZE }`;
        return this.http.get<AccountPagination>(url)
        .pipe(
            catchError(() => {
                this._status.set(AccountPaginationStatus.error);
                return of(null)
            }),
        );
    }
}