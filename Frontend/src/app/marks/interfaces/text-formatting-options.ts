import { NodeMark } from './node-mark';
import { Editor } from '@tiptap/core';
import { FloatingMenuOption } from '../../shared/components/layout/floating-menu/floating-menu-option';
import { Signal } from '@angular/core';

export function createTextFormattingOptions( editor: Signal<Editor | undefined> ): FloatingMenuOption[] {
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
      label: 'Align',
      icon: 'fa fa-align-left',
      children: [
        {
          label: 'Left',
          icon: 'fa fa-align-left',
          command: () => editor()?.chain().focus().setTextAlign('left').run(),
          isActive: () => {
            return isNodeMarkActive(editor(), {
              name: 'alignment',
              textAlign: 'left'
            });
          },
        },
        {
          label: 'Center',
          icon: 'fa fa-align-center',
          command: () => editor()?.chain().focus().setTextAlign('center').run(),
          isActive: () => {
            return isNodeMarkActive(editor(), {
              name: 'alignment',
              textAlign: 'center'
            });
          },
        },
        {
          label: 'Right',
          icon: 'fa fa-align-right',
          command: () => editor()?.chain().focus().setTextAlign('right').run(),
          isActive: () => {
            return isNodeMarkActive(editor(), {
              name: 'alignment',
              textAlign: 'right'
            });
          },
        },
        {
          label: 'Justify',
          icon: 'fa fa-align-justify',
          command: () => editor()?.chain().focus().setTextAlign('justify').run(),
          isActive: () => {
            return isNodeMarkActive(editor(), {
              name: 'alignment',
              textAlign: 'justify'
            });
          },
        },
      ],
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
      children: [
        {
          label: 'Yellow',
          icon: 'fa fa-circle',
          color: "#f5f378",
          command: () => editor()?.chain().focus().toggleHighlight({ color: '#f5f378' }).run(),
          isActive: () => {
            return isNodeMarkActive(editor(), {
              name: 'highlight',
              color: "#f5f378"
            })
          },
        },
        {
          label: 'Purple',
          icon: 'fa fa-circle',
          color: "#dcc1ff",
          command: () => editor()?.chain().focus().toggleHighlight({ color: '#dcc1ff' }).run(),
          isActive: () => {
            return isNodeMarkActive(editor(), {
              name: 'highlight',
              color: "#dcc1ff"
            })
          },
        },
        {
          label: 'Orange',
          icon: 'fa fa-circle',
          color: "#ec704b",
          command: () => editor()?.chain().focus().toggleHighlight({ color: '#ec704b' }).run(),
          isActive: () => {
            return isNodeMarkActive(editor(), {
              name: 'highlight',
              color: "#ec704b"
            })
          },
        },
      ]
    },
    {
      label: 'List',
      icon: 'fa fa-list',
      command: () => editor()?.chain().focus().toggleBulletList().run(),
      isActive: () => isNodeMarkActive(editor(), { name: 'bulletList' }),
    },
    {
      label: 'Code',
      icon: 'fa fa-code',
      command: () => editor()?.chain().focus().toggleCodeBlock().run(),
      isActive: () => isNodeMarkActive(editor(), { name: 'codeBlock' }),
    },
    {
      label: 'Task',
      icon: 'fa fa-square-check',
      command: () => editor()?.chain().focus().toggleTaskList().run(),
      isActive: () => isNodeMarkActive(editor(), { name: 'taskList' }),
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

        case 'highlight':
        if (color === undefined) return false;
        return editor.isActive(name, { color });

        default:
        return editor.isActive(name);
    }
}