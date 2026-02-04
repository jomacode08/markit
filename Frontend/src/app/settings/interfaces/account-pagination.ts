import { AccountSummary } from './account-summary';

export interface AccountPagination {
    data : AccountSummary[],
    meta : AccountPaginationMetaData
}

export interface AccountPaginationMetaData {
    totalItems : number,
    totalPages : number,
    currentPage : number,
    pageSize : number,
}