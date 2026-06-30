import { Injector } from "@angular/core";
import { mergeAttributes, Node } from '@tiptap/core';
import { AngularNodeViewRenderer } from 'ngx-tiptap';
import { GistManagerComponent } from "../components/gist-manager/gist-manager.component";

declare module '@tiptap/core' {
   interface Commands<ReturnType> {
      gistBlock: {
         insertGistBlock: () => ReturnType,
      }
   }
}

const GistBlockExtension = (injector : Injector): Node => {
   return Node.create({
      name: 'gistBlock',
      group: 'block',
      atom: true,
      draggable: true,
      selectable: true,

      addAttributes() {
         return {
            gistId: {
               default: null,
            },
            title: {
               default: null,
            },
         }
      },

      parseHTML() {
         return [
            { 
               tag: 'gist-block',
               getAttrs: (node) => {
                  return {
                     gistId : node.getAttribute('gistId'),
                     title : node.getAttribute('title'),
                  }
               }
            }
         ]
      },

      renderHTML({ HTMLAttributes }) {
         return ['gist-block', mergeAttributes(HTMLAttributes)]
      },

      renderMarkdown(node) {
         const gistId: string | null = node.attrs?.gistId;
         const title: string | null = node.attrs?.title;
         if (!gistId || !title) return '<gist-block/>';
         return `<gist-block gistId="${gistId}" title="${title}"/>`
      },

      addNodeView() {
         return AngularNodeViewRenderer(GistManagerComponent, {
            injector,
            attrs: {
               contenteditable: 'false',
            },
            // This ensures external changes
            // Tiptap will destroy and initialize the component.
            update: ({ oldNode, newNode }) => {
               return false;
            },
         })
      },

      addCommands() {
         return {
            insertGistBlock:
            () => ({ commands }) => {
               return commands.insertContent('<gist-block/>');
            }
         }
      }
   });
};

export default GistBlockExtension;
