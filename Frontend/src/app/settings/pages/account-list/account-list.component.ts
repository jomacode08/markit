import { ChangeDetectionStrategy, Component, Signal } from '@angular/core';

import { TableModule } from 'primeng/table';

import { AccountPaginationService, AccountPaginationStatus } from '../../services/account-pagination.service';
import { AccountSummary } from '../../interfaces/account-summary';
import { Column } from '../../../shared/components/layout/data-table.component/interfaces/column';
import { DataTableComponent } from '../../../shared/components/layout/data-table.component/data-table.component';

@Component({
  selector: 'app-account-list',
  standalone: true,
  imports: [TableModule, DataTableComponent],
  templateUrl: './account-list.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AccountListComponent {
  protected accounts : Signal<AccountSummary[]>;
  protected cols : Column[];
  protected status : Signal<AccountPaginationStatus>;
  protected totalItemsPerPage : Signal<number>;
  protected totalItems : Signal<number>;
  protected totalPages : Signal<number>;

  get accountPaginationStatus(): typeof AccountPaginationStatus {
    return AccountPaginationStatus;
  }

  constructor(private accountPaginationService: AccountPaginationService)
  {
    this.accounts = accountPaginationService.accounts;
    this.totalItemsPerPage = accountPaginationService.totalItemsPerPage;
    this.totalItems = accountPaginationService.totalItems;
    this.totalPages = accountPaginationService.totalPages;
    this.status = accountPaginationService.status;
    this.cols = [
      { field: 'id', header: 'Id' },
      { field: 'userName', header: 'User name' },
      { field: 'accessType', header: 'Access' },
      { 
        field: 'createdDate',
        header: 'Created at',
        transform: (value: Date) => {
          return new Date(value).toLocaleDateString(
            'en-US',
            { 
              year: 'numeric',
              month: 'long',
              day: 'numeric'
            }
          );
        }
      },
      {
        field: 'isLocked',
        header: 'Locked',
      },
      {
        field: 'isConfirmed',
        header: 'Confirmed',
        transform: (value: boolean) => {
          return value ? 'Yes' : 'No';
        } 
      },
    ];
  }

  public onPageSelected(pageIndex: number): void {
    this.accountPaginationService.loadPage(pageIndex);
  }
}