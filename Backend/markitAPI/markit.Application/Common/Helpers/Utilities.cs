using markit.Application.Features.Collections.Queries.ViewModels;
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

            if (content.Length == 0) return GeneralConstant.Marks.MARK_DEFAULT_PREVIEW;

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

        public static List<CollectionPath> CreateCollectionPath(Collection collection)
        {
            var path = new List<CollectionPath>();
            const string PATH_SPLITER = "/";
            string[] pathIds = collection.Path?.Split(PATH_SPLITER, StringSplitOptions.RemoveEmptyEntries) ?? [];
            string[] pathNames = collection.PathNames.Split(PATH_SPLITER, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < pathIds.Length; i++)
            {
                if (!int.TryParse(pathIds[i], out int collectionId))
                    throw new FormatException($"The path of the collection with id: {collection.Id} doesn't have the correct format.");

                path.Add(new CollectionPath
                {
                    CollectionId = collectionId,
                    Name = pathNames[i],
                    Emoji = i.Equals(pathIds.Length - 1) ? collection.Emoji : null
                });
            }

            return path;
        }

        public static DateTime? GetMostRecentBlockDate(ICollection<Block> blocks)
        {
            if (blocks.Count == 0) return null;
            return blocks.OrderBy(b => b.UpdatedDate).Last().UpdatedDate;
        }
    }
}
