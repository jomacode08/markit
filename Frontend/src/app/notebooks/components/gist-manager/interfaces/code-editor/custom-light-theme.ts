import { EditorView } from '@codemirror/view';
import { Extension } from '@codemirror/state';

export const customLightTheme: Extension = EditorView.theme({
  // Styles for the active line
  '.cm-activeLine': {
    backgroundColor: 'var(--p-secondary-100)',
  },
  // Styles for the gutter area (line numbers, fold markers, etc.)
  '.cm-gutters': {
    backgroundColor: 'var(--p-content-elevation-main)',
  },
  '.cm-activeLineGutter': {
    backgroundColor: 'var(--p-secondary-100)',
    color: 'var(--p-secondary-800)',
  }
});