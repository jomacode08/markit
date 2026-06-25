import { NodeMark } from './node-mark';
import { Editor } from '@tiptap/core';
import { FloatingMenuOption } from '../../shared/components/layout/floating-menu/floating-menu-option';
import { Signal } from '@angular/core';

interface TextFormattingOptionHandlers {
  openLinkDialog?(): void;
}

export function createTextFormattingOptions(
  editor: Signal<Editor | undefined>,
  handlers: TextFormattingOptionHandlers = {},
): FloatingMenuOption[] {
    return [
    {
      label: 'Bold',
      icon: 'fa fa-bold',
      command: () => editor()?.chain().focus().toggleBold().run(),
      isActive: () => isNodeMarkActive(editor(), { name: 'bold' }),
    },
    {
      label: 'Italic',
      icon: 'fa fa-italic',
      command: () => editor()?.chain().focus().toggleItalic().run(),
      isActive: () => isNodeMarkActive(editor(), { name: 'italic' }),
    },
    {
      label: 'Underline',
      icon: 'fa fa-underline',
      command: () => editor()?.chain().focus().toggleUnderline().run(),
      isActive: () => isNodeMarkActive(editor(), { name: 'underline' }),
    },
    {
      label: 'Heading',
      icon: 'fa fa-heading',
      children: [
        {
          label: 'Heading 1',
          icon: 'fa fa-heading',
          command: () => editor()?.chain().focus().toggleHeading({ level: 1 }).run(),
          isActive: () => {
            return isNodeMarkActive(editor(), {
              name: 'heading',
              level: 1
            });
          },
        },
        {
          label: 'Heading 2',
          icon: 'fa fa-heading',
          command: () => editor()?.chain().focus().toggleHeading({ level: 2 }).run(),
          isActive: () => {
            return isNodeMarkActive(editor(), {
              name: 'heading',
              level: 2
            });
          },
        },
        {
          label: 'Heading 3',
          icon: 'fa fa-heading',
          command: () => editor()?.chain().focus().toggleHeading({ level: 3 }).run(),
          isActive: () => {
            return isNodeMarkActive(editor(), {
              name: 'heading',
              level: 3
            });
          },
        },
      ],
    },
    {
      label: 'Highlight',
      icon: 'fa fa-highlighter',
      command: () => editor()?.chain().focus().toggleHighlight().run(),
      isActive: () => {
        return isNodeMarkActive(editor(), {
          name: 'highlight'
        })
      },
    },
    {
      label: 'Link',
      icon: 'fa fa-link',
      command: () => handlers.openLinkDialog?.(),
      isActive: () => isNodeMarkActive(editor(), { name: 'link' }),
      isDisabled: () => editor() === undefined,
    },
    {
      label: 'List',
      icon: 'fa fa-list',
      command: () => editor()?.chain().focus().toggleBulletList().run(),
      isActive: () => isNodeMarkActive(editor(), { name: 'bulletList' }),
    },
    {
      label: 'Table',
      icon: 'fa-solid fa-table',
      command: () => editor()?.chain().focus().insertTable().run(),
      isActive: () => isNodeMarkActive(editor(), { name: 'tableCell' })
        || isNodeMarkActive(editor(), { name: 'tableHeader' })
    },
    {
      label: 'Task',
      icon: 'fa fa-square-check',
      command: () => editor()?.chain().focus().toggleTaskList().run(),
      isActive: () => isNodeMarkActive(editor(), { name: 'taskList' }),
    },
    {
      label: 'Code',
      icon: 'fa fa-code',
      command: () => editor()?.chain().focus().toggleCodeBlock().run(),
      isActive: () => isNodeMarkActive(editor(), { name: 'codeBlock' }),
    },
    {
      label: 'Gist',
      icon: 'fa-brands fa-github',
      command: () => editor()?.chain().focus().insertGistBlock().run(),
      isActive: () => isNodeMarkActive(editor(), { name: 'gistBlock' }),
    },
  ];
};


function isNodeMarkActive(editor: Editor | undefined, nodeMark : NodeMark): boolean {
    const { name, level, color, textAlign } = nodeMark;

    if (editor === undefined || name === undefined) return false;

    switch (name) {
        case 'heading':
        if (level === undefined) return false;
        return editor.isActive(name, { level });

        case 'alignment':
        if (textAlign === undefined) return false;
        return editor.isActive({ textAlign });

        default:
        return editor.isActive(name);
    }
}