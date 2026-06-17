import { ChangeDetectionStrategy, Component } from '@angular/core';
import { AngularNodeViewComponent, TiptapNodeViewContentDirective } from 'ngx-tiptap';
import { TableMap } from '@tiptap/pm/tables';

@Component({
  selector: 'app-table-node-view',
  imports: [TiptapNodeViewContentDirective],
  template: `
    <div class="table-wrapper">
      <table>
        <tbody tiptapNodeViewContent></tbody>
      </table>
      <!-- Add column button -->
      <button
        type="button"
        class="add-col-btn"
        aria-label="Add column"
        contenteditable="false"
        (click)="addColumn()"
      >
        <span class="fa-solid fa-plus"></span>
      </button>
      <!-- Add row button -->
      <button
        type="button"
        class="add-row-btn"
        aria-label="Add row"
        contenteditable="false"
        (click)="addRow()"
      >
        <span class="fa-solid fa-plus"></span>
      </button>
    </div>
  `,
  styleUrl: './table-node-view.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TableNodeViewComponent extends AngularNodeViewComponent {
  public addColumn(): void {
    const map : TableMap = this.getTableMap();
    if (!this.isCellActive()) this.focusCell(0, map.width - 1);
    this.editor().chain().focus().addColumnAfter().run();
  }
  
  public addRow(): void {
    const map : TableMap = this.getTableMap();
    if (!this.isCellActive()) this.focusCell(map.height - 1, 0);
    this.editor().chain().focus().addRowAfter().run();
  }

  private getTableMap = (): TableMap => TableMap.get(this.node()); 
  
  private isCellActive(): boolean {
    const editor = this.editor();
    return editor.isActive('tableCell') || editor.isActive('tableHeader');
  }
  
  private focusCell(rowIndex: number, colIndex: number): void {
    const map: TableMap = this.getTableMap();
    const cellPosition = map.positionAt(rowIndex, colIndex, this.node());
    const tablePosition : number = this.getPos()() ?? -1;
  
    if (tablePosition < 0) return;
  
    this.editor().chain()
    .focus()
    .setTextSelection(tablePosition + cellPosition + 1)
    .run();
  }
}
