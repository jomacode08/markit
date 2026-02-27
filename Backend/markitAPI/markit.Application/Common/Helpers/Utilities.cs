using markit.Application.Features.Collections.Queries.ViewModels;
using markit.Application.Helpers;
using markit.Domain.Entities;
using System.Text.RegularExpressions;
using static markit.Application.Helpers.GeneralConstant;


namespace markit.Application.Common.Helpers
{
    public static class Utilities
    {
        public static string CreateMarkPreview(Mark mark)
        {
            Block? firstBlock = mark.Blocks?.FirstOrDefault();
            string content = firstBlock?.Content ?? "";
            int previewMaxLength = 40;

            if (content.Length == 0) return GeneralConstant.Marks.MARK_PLACEHOLDER;

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

        public static string? GetSystemConfigDescription(string key)
        {
            return key switch
            {
                SystemConfigKeys.IS_DEMO_ENABLED_KEY => "Configuration that toggles the demo features of the app.",
                SystemConfigKeys.DEMO_USER_ID_KEY => "The ID of the user designated for demo purposes.",
                SystemConfigKeys.DEMO_TOKEN_DURATION_IN_MINUTES_KEY => "The validity period (in minutes) for the demo access token.",
                _ => null
            };
        }
    }
}
