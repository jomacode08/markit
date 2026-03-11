import { ChangeDetectionStrategy, Component, EventEmitter, input, Output } from '@angular/core';

import { TableLazyLoadEvent, TableModule } from 'primeng/table';

import { Column } from './interfaces/column';
import { Action, ActionEvent } from './interfaces/action';

@Component({
    selector: 'shared-data-table',
    imports: [TableModule],
    styleUrl: './data-table.component.css',
    templateUrl: './data-table.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class DataTableComponent<T = unknown> {
  //* Inputs
  public data = input.required<T[]>();
  public columns = input.required<Column[]>();
  public loading = input.required<boolean>();
  public enablePagination = input<boolean>(false);
  public enableActions = input<boolean>(false);
  public rowsPerPage = input<number>(10);
  public totalPages = input<number>(1);
  public totalItems = input<number>(0);
  public customActions = input<Action[]>([
    {
      label: 'Edit',
      icon: 'fa fa-pencil'
    },
    {
      label: 'Delete',
      icon: 'fa fa-trash'
    },
  ]);
  //* Outputs
  // Emit the pageIndex of the selected page.
  @Output() public pageSelected = new EventEmitter<number>();
  // Emit the ActionEvent for the action that was triggered.
  @Output() public actionTriggered = new EventEmitter<ActionEvent<T>>();

  public loadDataLazy(event: TableLazyLoadEvent): void {
    if (event.first != undefined && event.rows != undefined) {
      const pageIndex = Math.floor(event.first / event.rows) + 1; 
      this.pageSelected.emit(pageIndex);
    }
  }

  public onActionButtonClick(action : Action, data: T): void {
    this.actionTriggered.emit({ action, data });
  }
}