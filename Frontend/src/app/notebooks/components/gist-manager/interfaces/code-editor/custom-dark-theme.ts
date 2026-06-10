import { EditorView } from '@codemirror/view';
import { Extension } from '@codemirror/state';

export const customDarkTheme: Extension = EditorView.theme({
  // Styles for the editor's main container
  '&': {
    backgroundColor: '#1a1b1e !important',
    fontSize: '.875rem',
  },
  // Styles for the active line
  '.cm-activeLine': {
    backgroundColor: '#232429 !important',
  },
  // Styles for line numbers
  '.cm-gutters': {
    backgroundColor: '#1a1b1e !important',
    color: '#3c3e43 !important',
  },
  '.cm-activeLineGutter': {
    backgroundColor: '#232429 !important',
    color: '#adb5bd !important',
  },
}, { dark: true });