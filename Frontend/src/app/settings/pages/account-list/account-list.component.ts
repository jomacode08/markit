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
import { AccountService } from '../../services/account.service';
import { Action, ActionEvent } from '../../../shared/components/layout/data-table.component/interfaces/action';

type DialogAction = 'Create' | 'Update';
enum DataTableAction {
  Edit = 'Edit'
}

@Component({
    selector: 'app-account-list',
    imports: [TableModule, DataTableComponent],
    providers: [DialogService],
    templateUrl: './account-list.component.html',
    styleUrl: './account-list.component.css',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class AccountListComponent implements OnDestroy {
  private dialogRef ?: DynamicDialogRef<AccountFormComponent> | null;
  protected accounts : Signal<AccountSummary[]>;
  protected cols : Column[];
  protected status : Signal<AccountPaginationStatus>;
  protected totalItemsPerPage : Signal<number>;
  protected totalItems : Signal<number>;
  protected totalPages : Signal<number>;
  protected dataTableActions : Action[] = [
    {
      label : DataTableAction.Edit,
      icon : 'fa fa-pencil'
    }
  ];

  get accountPaginationStatus(): typeof AccountPaginationStatus {
    return AccountPaginationStatus;
  }

  constructor(
    private accountService : AccountService,
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
        field: 'enabled',
        header: 'Enabled',
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

  public onActionTriggered(event : ActionEvent<AccountSummary>): void {
    const action = event.action.label;
    switch(action){
      case DataTableAction.Edit :
        this.EditAccount(event.data);
        break;
    }
  }

  public EditAccount(data: AccountSummary): void {
    const { id } = data;
    this.accountService.getByUserId(id).subscribe({
      next: (account) => this.showAccountFormDialog('Update', account),
      error : () => this.messageService.showGeneralError('Failed to load account details.')
    });
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

    this.dialogRef?.onClose.subscribe((result : Account) => {
      if (result) {
        this.accountPaginationService.loadPage(1);
        this.messageService.showGeneralSuccess(CONFIRMATION_MESSAGE);
      }
    });
  }
}