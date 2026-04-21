using Markdig;
using markit.Application.Common.Exceptions;
using markit.Application.Models.Authentication.Enums;
using markit.Application.Features.Collections.Queries.ViewModels;
using markit.Domain.Entities;
using System.Text.RegularExpressions;
using static markit.Application.Helpers.GeneralConstant;

namespace markit.Application.Common.Helpers
{
    public static partial class Extension
    {
        [GeneratedRegex(@"^\s*(.+)", RegexOptions.None)]
        private static partial Regex FirstLineRegex();

        public static string ConstructPreview(this Mark mark)
        {
            Block? firstBlock = mark.Blocks?.FirstOrDefault();
            string markdown = firstBlock?.Content ?? "";
            int previewMaxLength = 40;

            if (string.IsNullOrEmpty(markdown)) return Marks.MARK_PLACEHOLDER;
            Match match = FirstLineRegex().Match(markdown);
            if (!match.Success || string.IsNullOrEmpty(match.Groups[1].Value)) return Marks.MARK_PLACEHOLDER;

            string preview = Markdown.ToPlainText(match.Groups[1].Value);
            if (string.IsNullOrEmpty(preview)) return Marks.MARK_PLACEHOLDER;
            return preview.Length > previewMaxLength
                ? $"{preview[..previewMaxLength]}..."
                : preview;
        }

        public static List<CollectionPath> CreatePath(this Collection collection)
        {
            var path = new List<CollectionPath>();
            const string PATH_SPLITTER = "/";
            string[] pathIds = collection.Path?.Split(PATH_SPLITTER, StringSplitOptions.RemoveEmptyEntries) ?? [];
            string[] pathNames = collection.PathNames.Split(PATH_SPLITTER, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < pathIds.Length; i++)
            {
                if (!int.TryParse(pathIds[i], out int collectionId))
                    throw new FormatException($"The path of the collection with id: {collection.Id} doesn't have the correct format.");

                path.Add(new CollectionPath
                {
                    CollectionId = collectionId,
                    Name = pathNames[i],
                    Emoji = i == (pathIds.Length - 1) ? collection.Emoji : null
                });
            }

            return path;
        }

        public static string GetName(this LoginProvider loginProvider)
        {
            return Enum.GetName(typeof(LoginProvider), loginProvider)
                ?? throw new InvalidOperationException();
        }

        public static string GetName(this LoginPurpose loginPurpose)
        {
            return Enum.GetName(typeof(LoginPurpose), loginPurpose)
                ?? throw new InvalidOperationException();
        }

        public static void ValidateCreator(this Collection collection, int creatorId)
        {
            if (collection.CreatorId != creatorId)
            {
                throw new ForbiddenResourceException(
                    resource: "Collection",
                    resourceId: collection.Id,
                    creatorId
                );
            }
        }

        public static void ValidateCreator(this Mark mark, int creatorId)
        {
            ArgumentNullException.ThrowIfNull(mark.Collection, nameof(mark.Collection));
            if (mark.Collection.CreatorId != creatorId)
            {
                throw new ForbiddenResourceException(
                    resource: "Mark",
                    resourceId: mark.Id,
                    creatorId
                );
            }
        }

        public static DateTime? GetMostRecentBlockDate(this ICollection<Block> blocks)
        {
            if (blocks.Count == 0) return null;
            return blocks.Max(b => b.UpdatedDate);
        }
    }
}
