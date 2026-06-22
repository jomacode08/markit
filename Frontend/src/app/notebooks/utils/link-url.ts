/**
 * Accepts web URLs with an optional http/https protocol.
 *
 * Examples accepted:
 * - `example.com`
 * - `www.example.com/path`
 * - `https://example.com?q=markit`
 *
 * Examples intentionally rejected:
 * - `mailto:user@example.com`
 * - `javascript:alert(1)`
 * - relative paths like `/notebooks/1`
 */
const WEB_URL_PATTERN = /^(https?:\/\/)?([a-zA-Z0-9-]+\.)+[a-zA-Z]{2,}(?::\d{2,5})?(?:[/?#][^\s]*)?$/;

/**
 * Normalizes user-entered web URLs to the canonical href stored in the editor.
 *
 * Bare domains are treated as secure web links and receive `https://`. Existing
 * `http://` and `https://` links are preserved as entered.
 */
export function normalizeWebUrl(url: string): string {
  const trimmedUrl = url.trim();

  if (/^https?:\/\//i.test(trimmedUrl)) {
    return trimmedUrl;
  }

  return `https://${trimmedUrl}`;
}

/**
 * Validates the app's notebook link policy.
 *
 * This helper is shared by the Tiptap Link extension, the markdown link input
 * rule, and the link dialog validator so all link entry points accept and
 * reject the same URLs.
 */
export function isWebUrl(url: string): boolean {
  const trimmedUrl = url.trim();

  if (!WEB_URL_PATTERN.test(trimmedUrl)) {
    return false;
  }

  try {
    const normalizedUrl = normalizeWebUrl(trimmedUrl);
    const parsedUrl = new URL(normalizedUrl);
    return parsedUrl.protocol === 'http:' || parsedUrl.protocol === 'https:';
  } catch {
    return false;
  }
}
