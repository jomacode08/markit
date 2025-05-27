using markit.Application.Helpers;
using markit.Domain.Entities;
using System.Text.RegularExpressions;

namespace markit.Application.Common.Helpers
{
    public static class Utilities
    {
        public static string CreateMarkPreview(Mark mark)
        {
            Block? firstBlock = mark.Blocks?.FirstOrDefault();
            string content = firstBlock?.Content ?? "";
            int previewMaxLength = 40;

            if (content.Length == 0) return GeneralConstant.Marks.COLLECTION_DEFAULT_PREVIEW;

            // Find the first closable html tag in the block content.
            string firstTagElement = Regex.Match(content, "<([a-zA-Z][a-zA-Z0-9]*)\\b[^>]*>(.*?)<\\/\\1>").Value;

            // Get inner text from the tag.
            string innerText = Regex.Replace(firstTagElement, "<.*?>", string.Empty);

            // Set preview maxlength
            innerText = innerText.Length > previewMaxLength
                ? innerText[..previewMaxLength]
                : innerText;

            return $"{innerText}...";
        }
    }
}
