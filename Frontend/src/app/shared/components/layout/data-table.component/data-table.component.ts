import { ChangeDetectionStrategy, Component, EventEmitter, input, Output } from '@angular/core';

import { TableLazyLoadEvent, TableModule } from 'primeng/table';

import { Column } from './interfaces/column';

@Component({
  selector: 'shared-data-table',
  standalone: true,
  imports: [TableModule],
  styleUrl: './data-table.component.css',
  templateUrl: './data-table.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
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
  //* Outputs
  // Emit the pageIndex of the selected page.
  @Output() public pageSelected = new EventEmitter<number>();
  // Emit the data for the row that was clicked by the action button.
  @Output() public edit = new EventEmitter<T>();
  @Output() public delete = new EventEmitter<T>();

  public loadDataLazy(event: TableLazyLoadEvent){
    if (event.first != undefined && event.rows != undefined) {
      const pageIndex = Math.floor(event.first / event.rows) + 1; 
      this.pageSelected.emit(pageIndex);
    }
  }
}