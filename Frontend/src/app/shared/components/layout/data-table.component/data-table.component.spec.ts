import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';

import { TableLazyLoadEvent } from 'primeng/table';

import { DataTableComponent } from './data-table.component';
import { Column } from './interfaces/column';

describe('DataTableComponent', () => {
  let component: DataTableComponent<any>;
  let fixture: ComponentFixture<DataTableComponent<any>>;
  let compiled: HTMLElement;

  // Sample test data
  const mockColumns: Column[] = [
    { field: 'id', header: 'ID' },
    { field: 'name', header: 'Name' },
    { field: 'email', header: 'Email' },
  ];

  const mockData = [
    { id: 1, name: 'John Doe', email: 'john@example.com' },
    { id: 2, name: 'Jane Smith', email: 'jane@example.com' },
    { id: 3, name: 'Bob Johnson', email: 'bob@example.com' },
  ];

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DataTableComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(DataTableComponent);
    component = fixture.componentInstance;
    compiled = fixture.nativeElement;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('Component Inputs', () => {
    // Set default required input values
    beforeEach(() => {
      fixture.componentRef.setInput('data', []);
      fixture.componentRef.setInput('columns', []);
      fixture.componentRef.setInput('loading', false);
    });

    it('should set data input correctly', () => {
      fixture.componentRef.setInput('data', mockData);
      fixture.detectChanges();

      expect(component.data()).toEqual(mockData);
    });

    it('should set columns input correctly', () => {
      fixture.componentRef.setInput('columns', mockColumns);
      fixture.detectChanges();

      expect(component.columns()).toEqual(mockColumns);
    });

    it('should set loading input correctly', () => {
      fixture.componentRef.setInput('loading', true);
      fixture.detectChanges();

      expect(component.loading()).toBe(true);
    });

    it('should set enablePagination input with default value false', () => {
      expect(component.enablePagination()).toBe(false);
    });

    it('should set enablePagination input correctly', () => {
      fixture.componentRef.setInput('enablePagination', true);
      fixture.detectChanges();

      expect(component.enablePagination()).toBe(true);
    });

    it('should set enableActions input with default value false', () => {
      expect(component.enableActions()).toBe(false);
    });

    it('should set enableActions input correctly', () => {
      fixture.componentRef.setInput('enableActions', true);
      fixture.detectChanges();

      expect(component.enableActions()).toBe(true);
    });

    it('should set rowsPerPage input with default value 10', () => {
      expect(component.rowsPerPage()).toBe(10);
    });

    it('should set rowsPerPage input correctly', () => {
      fixture.componentRef.setInput('rowsPerPage', 25);
      fixture.detectChanges();

      expect(component.rowsPerPage()).toBe(25);
    });

    it('should set totalPages input with default value 1', () => {
      expect(component.totalPages()).toBe(1);
    });

    it('should set totalPages input correctly', () => {
      fixture.componentRef.setInput('totalPages', 5);
      fixture.detectChanges();

      expect(component.totalPages()).toBe(5);
    });

    it('should set totalItems input with default value 0', () => {
      expect(component.totalItems()).toBe(0);
    });

    it('should set totalItems input correctly', () => {
      fixture.componentRef.setInput('totalItems', 100);
      fixture.detectChanges();

      expect(component.totalItems()).toBe(100);
    });
  });

  describe('Component Outputs', () => {
    it('should have pageSelected output emitter', () => {
      expect(component.pageSelected).toBeDefined();
    });

    it('should have edit output emitter', () => {
      expect(component.edit).toBeDefined();
    });

    it('should have delete output emitter', () => {
      expect(component.delete).toBeDefined();
    });

    it('should emit pageSelected when page changes', (done) => {
      component.pageSelected.subscribe((pageIndex: number) => {
        expect(pageIndex).toBe(2);
        done();
      });

      component.pageSelected.emit(2);
    });

    it('should emit edit with row data', (done) => {
      const testData = mockData[0];
      component.edit.subscribe((data: any) => {
        expect(data).toEqual(testData);
        done();
      });

      component.edit.emit(testData);
    });

    it('should emit delete with row data', (done) => {
      const testData = mockData[1];
      component.delete.subscribe((data: any) => {
        expect(data).toEqual(testData);
        done();
      });

      component.delete.emit(testData);
    });
  });

  describe('loadDataLazy method', () => {
    it('should emit correct page index when first is 0 and rows is 10', () => {
      const event: TableLazyLoadEvent = {
        first: 0,
        rows: 10,
      };

      spyOn(component.pageSelected, 'emit');
      component.loadDataLazy(event);

      expect(component.pageSelected.emit).toHaveBeenCalledWith(1);
    });

    it('should emit correct page index when first is 10 and rows is 10', () => {
      const event: TableLazyLoadEvent = {
        first: 10,
        rows: 10,
      };

      spyOn(component.pageSelected, 'emit');
      component.loadDataLazy(event);

      expect(component.pageSelected.emit).toHaveBeenCalledWith(2);
    });

    it('should emit correct page index when first is 20 and rows is 10', () => {
      const event: TableLazyLoadEvent = {
        first: 20,
        rows: 10,
      };

      spyOn(component.pageSelected, 'emit');
      component.loadDataLazy(event);

      expect(component.pageSelected.emit).toHaveBeenCalledWith(3);
    });

    it('should emit correct page index with different rows per page (25)', () => {
      const event: TableLazyLoadEvent = {
        first: 50,
        rows: 25,
      };

      spyOn(component.pageSelected, 'emit');
      component.loadDataLazy(event);

      expect(component.pageSelected.emit).toHaveBeenCalledWith(3);
    });

    it('should not emit when first is undefined', () => {
      const event: TableLazyLoadEvent = {
        first: undefined,
        rows: 10,
      };

      spyOn(component.pageSelected, 'emit');
      component.loadDataLazy(event);

      expect(component.pageSelected.emit).not.toHaveBeenCalled();
    });

    it('should not emit when rows is undefined', () => {
      const event: TableLazyLoadEvent = {
        first: 10,
        rows: undefined,
      };

      spyOn(component.pageSelected, 'emit');
      component.loadDataLazy(event);

      expect(component.pageSelected.emit).not.toHaveBeenCalled();
    });

    it('should not emit when both first and rows are undefined', () => {
      const event: TableLazyLoadEvent = {
        first: undefined,
        rows: undefined,
      };

      spyOn(component.pageSelected, 'emit');
      component.loadDataLazy(event);

      expect(component.pageSelected.emit).not.toHaveBeenCalled();
    });
  });

  describe('Template Rendering', () => {
    beforeEach(() => {
      fixture.componentRef.setInput('data', mockData);
      fixture.componentRef.setInput('columns', mockColumns);
      fixture.componentRef.setInput('loading', false);
    });

    it('should render p-table component', () => {
      fixture.detectChanges();
      const pTable = compiled.querySelector('p-table');
      expect(pTable).toBeTruthy();
    });

    it('should render column headers', () => {
      fixture.detectChanges();
      const headers = compiled.querySelectorAll('th');
      
      expect(headers.length).toBe(mockColumns.length);
      expect(headers[0].textContent?.trim()).toBe('ID');
      expect(headers[1].textContent?.trim()).toBe('Name');
      expect(headers[2].textContent?.trim()).toBe('Email');
    });

    it('should render data rows', () => {
      fixture.detectChanges();
      const rows = compiled.querySelectorAll('tbody tr');
      
      expect(rows.length).toBeGreaterThanOrEqual(mockData.length);
    });

    it('should render cell data correctly', () => {
      fixture.detectChanges();
      const firstRowCells = compiled.querySelectorAll('tbody tr:first-child td');
      
      expect(firstRowCells[0].textContent?.trim()).toBe('1');
      expect(firstRowCells[1].textContent?.trim()).toBe('John Doe');
      expect(firstRowCells[2].textContent?.trim()).toBe('john@example.com');
    });

    it('should display "-" for null or undefined values', () => {
      const dataWithNull = [{ id: 1, name: null, email: undefined }];
      fixture.componentRef.setInput('data', dataWithNull);
      fixture.detectChanges();

      const cells = compiled.querySelectorAll('tbody tr:first-child td');
      expect(cells[1].textContent?.trim()).toBe('-');
      expect(cells[2].textContent?.trim()).toBe('-');
    });

    it('should apply transform function when provided', () => {
      const columnsWithTransform: Column[] = [
        { field: 'id', header: 'ID', transform: (val) => `#${val}` },
        { field: 'name', header: 'Name' },
      ];
      
      fixture.componentRef.setInput('columns', columnsWithTransform);
      fixture.detectChanges();

      const firstCell = compiled.querySelector('tbody tr:first-child td');
      expect(firstCell?.textContent?.trim()).toBe('#1');
    });

    it('should show empty message when no data', () => {
      fixture.componentRef.setInput('data', []);
      fixture.detectChanges();

      const emptyMessage = compiled.querySelector('.text-center');
      expect(emptyMessage?.textContent?.trim()).toBe('No records found');
    });
  });

  describe('Actions Column', () => {
    beforeEach(() => {
      fixture.componentRef.setInput('data', mockData);
      fixture.componentRef.setInput('columns', mockColumns);
      fixture.componentRef.setInput('loading', false);
    });

    it('should not render Actions header when enableActions is false', () => {
      fixture.componentRef.setInput('enableActions', false);
      fixture.detectChanges();

      const headers = compiled.querySelectorAll('th');
      const actionsHeader = Array.from(headers).find(h => h.textContent?.trim() === 'Actions');
      expect(actionsHeader).toBeFalsy();
    });

    it('should render Actions header when enableActions is true', () => {
      fixture.componentRef.setInput('enableActions', true);
      fixture.detectChanges();

      const headers = compiled.querySelectorAll('th');
      const actionsHeader = Array.from(headers).find(h => h.textContent?.trim() === 'Actions');
      expect(actionsHeader).toBeTruthy();
    });

    it('should render edit and delete buttons when enableActions is true', () => {
      fixture.componentRef.setInput('enableActions', true);
      fixture.detectChanges();

      const editButton = compiled.querySelector('button[aria-label="Edit"]');
      const deleteButton = compiled.querySelector('button[aria-label="Delete"]');

      expect(editButton).toBeTruthy();
      expect(deleteButton).toBeTruthy();
    });

    it('should emit edit event when edit button is clicked', () => {
      fixture.componentRef.setInput('enableActions', true);
      fixture.detectChanges();

      spyOn(component.edit, 'emit');

      const editButton = compiled.querySelector('button[aria-label="Edit"]') as HTMLButtonElement;
      editButton?.click();

      expect(component.edit.emit).toHaveBeenCalledWith(mockData[0]);
    });

    it('should emit delete event when delete button is clicked', () => {
      fixture.componentRef.setInput('enableActions', true);
      fixture.detectChanges();

      spyOn(component.delete, 'emit');

      const deleteButton = compiled.querySelector('button[aria-label="Delete"]') as HTMLButtonElement;
      deleteButton?.click();

      expect(component.delete.emit).toHaveBeenCalledWith(mockData[0]);
    });
  });

  describe('Pagination', () => {
    beforeEach(() => {
      fixture.componentRef.setInput('data', mockData);
      fixture.componentRef.setInput('columns', mockColumns);
      fixture.componentRef.setInput('loading', false);
    });

    it('should pass pagination enabled to p-table when enablePagination is true', () => {
      fixture.componentRef.setInput('enablePagination', true);
      fixture.detectChanges();

      const pTableDebug = fixture.debugElement.query(By.css('p-table'));
      expect(pTableDebug.componentInstance.paginator).toBe(true);
    });

    it('should pass pagination disabled to p-table when enablePagination is false', () => {
      fixture.componentRef.setInput('enablePagination', false);
      fixture.detectChanges();

      const pTableDebug = fixture.debugElement.query(By.css('p-table'));
      expect(pTableDebug.componentInstance.paginator).toBe(false);
    });

    it('should pass lazy loading enabled when enablePagination is true', () => {
      fixture.componentRef.setInput('enablePagination', true);
      fixture.detectChanges();

      const pTableDebug = fixture.debugElement.query(By.css('p-table'));
      expect(pTableDebug.componentInstance.lazy).toBe(true);
    });

    it('should pass totalRecords to p-table', () => {
      fixture.componentRef.setInput('enablePagination', true);
      fixture.componentRef.setInput('totalItems', 100);
      fixture.detectChanges();

      const pTableDebug = fixture.debugElement.query(By.css('p-table'));
      expect(pTableDebug.componentInstance.totalRecords).toBe(100);
    });

    it('should pass rows per page to p-table', () => {
      fixture.componentRef.setInput('rowsPerPage', 25);
      fixture.detectChanges();

      const pTableDebug = fixture.debugElement.query(By.css('p-table'));
      expect(pTableDebug.componentInstance.rows).toBe(25);
    });
  });

  describe('Loading State', () => {
    beforeEach(() => {
      fixture.componentRef.setInput('data', mockData);
      fixture.componentRef.setInput('columns', mockColumns);
    });

    it('should pass loading state to p-table when true', () => {
      fixture.componentRef.setInput('loading', true);
      fixture.detectChanges();

      const pTableDebug = fixture.debugElement.query(By.css('p-table'));
      expect(pTableDebug.componentInstance.loading).toBe(true);
    });

    it('should pass loading state to p-table when false', () => {
      fixture.componentRef.setInput('loading', false);
      fixture.detectChanges();

      const pTableDebug = fixture.debugElement.query(By.css('p-table'));
      expect(pTableDebug.componentInstance.loading).toBe(false);
    });
  });

  describe('Integration Tests', () => {
    it('should handle complete workflow with pagination and actions', () => {
      fixture.componentRef.setInput('data', mockData);
      fixture.componentRef.setInput('columns', mockColumns);
      fixture.componentRef.setInput('loading', false);
      fixture.componentRef.setInput('enablePagination', true);
      fixture.componentRef.setInput('enableActions', true);
      fixture.componentRef.setInput('rowsPerPage', 10);
      fixture.componentRef.setInput('totalItems', 30);
      fixture.detectChanges();

      // Verify table is rendered with all features
      const headers = compiled.querySelectorAll('th');
      expect(headers.length).toBe(mockColumns.length + 1); // +1 for Actions column

      const editButton = compiled.querySelector('button[aria-label="Edit"]');
      const deleteButton = compiled.querySelector('button[aria-label="Delete"]');
      expect(editButton).toBeTruthy();
      expect(deleteButton).toBeTruthy();

      const pTableDebug = fixture.debugElement.query(By.css('p-table'));
      expect(pTableDebug.componentInstance.paginator).toBe(true);
      expect(pTableDebug.componentInstance.totalRecords).toBe(30);
    });

    it('should handle empty data with pagination and actions enabled', () => {
      fixture.componentRef.setInput('data', []);
      fixture.componentRef.setInput('columns', mockColumns);
      fixture.componentRef.setInput('loading', false);
      fixture.componentRef.setInput('enablePagination', true);
      fixture.componentRef.setInput('enableActions', true);
      fixture.detectChanges();

      const emptyMessage = compiled.querySelector('.text-center');
      expect(emptyMessage?.textContent?.trim()).toBe('No records found');
    });

    it('should handle data changes dynamically', () => {
      fixture.componentRef.setInput('data', mockData);
      fixture.componentRef.setInput('columns', mockColumns);
      fixture.componentRef.setInput('loading', false);
      fixture.detectChanges();

      let rows = compiled.querySelectorAll('tbody tr');
      expect(rows.length).toBeGreaterThanOrEqual(mockData.length);

      // Update data
      const newData = [{ id: 4, name: 'Alice Wonder', email: 'alice@example.com' }];
      fixture.componentRef.setInput('data', newData);
      fixture.detectChanges();

      const firstRowCells = compiled.querySelectorAll('tbody tr:first-child td');
      expect(firstRowCells[0].textContent?.trim()).toBe('4');
      expect(firstRowCells[1].textContent?.trim()).toBe('Alice Wonder');
    });
  });

  describe('Edge Cases', () => {
    it('should handle very large datasets', () => {
      const largeData = Array.from({ length: 1000 }, (_, i) => ({
        id: i + 1,
        name: `User ${i + 1}`,
        email: `user${i + 1}@example.com`,
      }));

      fixture.componentRef.setInput('data', largeData);
      fixture.componentRef.setInput('columns', mockColumns);
      fixture.componentRef.setInput('loading', false);
      fixture.detectChanges();

      // Should render without crashing
      const pTable = compiled.querySelector('p-table');
      expect(pTable).toBeTruthy();
    });

    it('should handle empty string as a valid value', () => {
      const dataWithEmpty = [{ id: 1, name: '', email: 'test@test.com' }];
      fixture.componentRef.setInput('data', dataWithEmpty);
      fixture.componentRef.setInput('columns', mockColumns);
      fixture.componentRef.setInput('loading', false);
      fixture.detectChanges();

      const nameCell = compiled.querySelectorAll('tbody tr:first-child td')[1];
      expect(nameCell?.textContent?.trim()).toBe('');
    });
  });

  describe('Change Detection', () => {
    it('should update view when input signals change', () => {
      fixture.componentRef.setInput('data', mockData);
      fixture.componentRef.setInput('columns', mockColumns);
      fixture.componentRef.setInput('loading', false);
      fixture.detectChanges();

      const initialRows = compiled.querySelectorAll('tbody tr').length;

      const newData = [...mockData, { id: 4, name: 'New User', email: 'new@test.com' }];
      fixture.componentRef.setInput('data', newData);
      fixture.detectChanges();

      const updatedRows = compiled.querySelectorAll('tbody tr').length;
      expect(updatedRows).toBeGreaterThanOrEqual(initialRows);
    });
  });
});