import { Extension, InputRule } from '@tiptap/core';
import type { MarkType } from '@tiptap/pm/model';

import { isWebUrl, normalizeWebUrl } from '../utils/link-url';

/**
 * Matches a markdown link typed at the end of the current text input.
 *
 * Capture groups:
 * 1. Full markdown link syntax, for example `[Markit](markit.dev)`.
 * 2. Link text, for example `Markit`.
 * 3. Link URL, for example `markit.dev`.
 *
 * The leading `(?:^|\s)` lets users type links at the beginning of a block or
 * after whitespace while keeping the whitespace outside the replacement range.
 */
const MARKDOWN_LINK_INPUT_PATTERN = /(?:^|\s)(\[([^\]]+)\]\(([^)\s]+)\))$/;

/**
 * Converts typed markdown links into Tiptap link marks.
 *
 * Tiptap's Markdown extension parses `[text](url)` when content is loaded from
 * markdown, but it does not automatically transform that syntax while users are
 * typing in the editor. This input rule fills that gap so the editing
 * experience matches the persisted markdown format.
 */
export const MarkdownLinkInputRule = Extension.create({
  name: 'markdownLinkInputRule',

  addInputRules() {
    return [
      new InputRule({
        find: MARKDOWN_LINK_INPUT_PATTERN,
        handler: ({ state, range, match }) => {
          const markdownSyntax: string = match[1];
          const text: string = match[2];
          const url: string = match[3];
          
          // Keep invalid or unsupported URLs as plain text instead of deleting what the user typed.
          if (!markdownSyntax || !text || !url || !isWebUrl(url)) {
            return null;
          }
          
          const linkMark: MarkType = state.schema.marks['link'];
          
          if (!linkMark) {
            return null;
          }
          
          // Only replace the `[text](url)` syntax, not any whitespace that
          // preceded it and was consumed by the input rule match.
          const from: number = range.to - markdownSyntax.length;
          const linkText = state.schema.text(text, [
            linkMark.create({ href: normalizeWebUrl(url) }),
          ]);
          
          // The transaction has been mutated above. Returning undefined (or no explicit
          // value) tells Tiptap to dispatch the modified transaction.
          state.tr.replaceWith(from, range.to, linkText);
          return;
        },
      }),
    ];
  },
});