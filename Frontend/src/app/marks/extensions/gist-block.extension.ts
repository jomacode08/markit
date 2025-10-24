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

      addNodeView() {
         return AngularNodeViewRenderer(GistManagerComponent, { injector })
      },

      addCommands() {
         return {
            insertGistBlock:
            () => ({ commands }) => {
               return commands.insertContent('<gist-block></gist-block>');
            }
         }
      }
   });
};

export default GistBlockExtension;