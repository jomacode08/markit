import { Editor } from "@tiptap/core";
import { CellSelection } from "prosemirror-tables";

export function deleteTableSelection(editor: Editor): boolean {
    const { selection } = editor.state;
    if (!(selection instanceof CellSelection)) return false;

    const isCol = selection.isColSelection();
    const isRow = selection.isRowSelection();

    if (isCol && isRow) return editor.chain().focus().deleteTable().run();
    if (isCol) return editor.chain().focus().deleteColumn().run();
    if (isRow) return editor.chain().focus().deleteRow().run();
    return false;
}