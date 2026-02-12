import { ChangeDetectionStrategy, Component, OnDestroy, Signal } from '@angular/core';

import { TableModule } from 'primeng/table';
import { DialogService, DynamicDialogRef } from 'primeng/dynamicdialog';

import { Account } from '../../interfaces/account';
import { AccountFormComponent } from './components/account-form/account-form.component';
import { AccountPaginationService, AccountPaginationStatus } from '../../services/account-pagination.service';
import { AccountSummary } from '../../interfaces/account-summary';
import { Column } from '../../../shared/components/layout/data-table.component/interfaces/column';
import { CustomMessageService } from '../../../shared/services/custom-message.service';
import { DataTableComponent } from '../../../shared/components/layout/data-table.component/data-table.component';

type DialogAction = 'Create' | 'Update';

@Component({
  selector: 'app-account-list',
  standalone: true,
  imports: [TableModule, DataTableComponent],
  providers: [DialogService],
  templateUrl: './account-list.component.html',
  styleUrl: './account-list.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AccountListComponent implements OnDestroy {
  private dialogRef ?: DynamicDialogRef;
  protected accounts : Signal<AccountSummary[]>;
  protected cols : Column[];
  protected status : Signal<AccountPaginationStatus>;
  protected totalItemsPerPage : Signal<number>;
  protected totalItems : Signal<number>;
  protected totalPages : Signal<number>;

  get accountPaginationStatus(): typeof AccountPaginationStatus {
    return AccountPaginationStatus;
  }

  constructor(
    private accountPaginationService: AccountPaginationService,
    private dialogService : DialogService,
    private messageService : CustomMessageService,
  )
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

  public ngOnDestroy(): void {
    if (this.dialogRef) this.dialogRef.destroy();
  }

  public onPageSelected(pageIndex: number): void {
    this.accountPaginationService.loadPage(pageIndex);
  }

  public onAddAccountBtnClick(): void {
    this.showAccountFormDialog('Create');
  }

  private showAccountFormDialog(action : DialogAction, accountData ?: Account): void {
    if (action === 'Update' && !accountData) return;
    const HEADER = `${ action } Account`;
    const CONFIRMATION_MESSAGE = `The account was ${ action === 'Create' ? 'created' : 'updated' } successfully.`;

    this.dialogRef = this.dialogService.open(AccountFormComponent, {
      header : HEADER,
      width: '500px',
      styleClass: 'custom-dialog',
      data: accountData ?? {}
    });

    this.dialogRef.onClose.subscribe((result : Account) => {
      if (result) {
        this.accountPaginationService.loadPage(1);
        this.messageService.showGeneralSuccess(CONFIRMATION_MESSAGE);
      }
    });
  }
}