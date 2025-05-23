using System.Text.RegularExpressions;
using markit.Application.Contracts.Persistence.Marks;
using markit.Application.Features.Collections.Queries.ViewModels;
using markit.Domain.Entities;
using markit.Infraestructure.Persistence.EF;
using Microsoft.EntityFrameworkCore;
using markit.Application.Helpers;

namespace markit.Infraestructure.Repositorys.Marks
{
    public class CollectionRepository : BaseRepository<Collection>, ICollectionRepository
    {
        public CollectionRepository(MarkitDbContext mirefDbContext) : base(mirefDbContext)
        {
        }

        public async Task<List<Collection>> GetHierarchyRecursively(int rootCollectionId)
        {
			var collections = context
				.Collections
				.FromSql($@"
					WITH CollectionHierarchy AS (
						SELECT 
							c1.Id, c1.Name, c1.Path, c1.IsMain, c1.ParentId, c1.CreatorId,
							c1.CreatedDate, c1.CreatedBy, c1.UpdatedDate, c1.UpdatedBy, c1.Enable
						FROM Collections as c1
						WHERE c1.Id = { rootCollectionId } AND Enable = 1

						UNION ALL

						SELECT
							c2.Id, c2.Name, c2.Path, c2.IsMain, c2.ParentId, c2.CreatorId,
							c2.CreatedDate, c2.CreatedBy, c2.UpdatedDate, c2.UpdatedBy, c2.Enable
						FROM Collections AS c2
						INNER JOIN CollectionHierarchy ch ON c2.ParentId = ch.Id
						WHERE c2.Enable = 1
					)
					SELECT * FROM CollectionHierarchy;
                ").IgnoreQueryFilters();

            return await collections.ToListAsync();
        }

        // Get child collections and marks of a specific collection.
        public async Task<List<CollectionItem>> GetCollectionItems(int collectionId)
        {

            var collections = await context
                .Collections
                .Include(c => c.Marks)
                .Where(c => c.ParentId == collectionId)
                .Select(c => new CollectionItem
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = c.Name,
                    CollectionId = collectionId,
                    Type = CollectionItemType.Collection,
                    TypeId = c.Id,
                    Preview = c.Marks != null
                        ? $"{c.Marks.Count} marks"
                        : GeneralConstant.Marks.MARK_DEFAULT_PREVIEW,
                })
                .ToListAsync();


            var marks = await context
                .Marks
                .Where(m => m.CollectionId == collectionId)
                .Select(m => new CollectionItem
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = m.Name,
                    CollectionId = collectionId,
                    Type = CollectionItemType.Mark,
                    TypeId = m.Id,
                    UpdateDate = m.UpdatedDate ?? (DateTime)m.CreatedDate!,
                    Preview = m.Blocks != null ? GetMarkPreview(m.Blocks) : "",
                })
                .ToListAsync();

            return [.. collections, .. marks];
        }

        private static string GetMarkPreview(ICollection<Block> blocks)
        {
            var firstBlock = blocks.FirstOrDefault();
            string content = firstBlock?.Content ?? "";
            int previewMaxLength = 40;

            if (content.Length == 0) return GeneralConstant.Marks.COLLECTION_DEFAULT_PREVIEW;

            // Find the first closable html tag in the block content.
            string firstTagElement = Regex.Match(content, "<([a-zA-Z][a-zA-Z0-9]*)\\b[^>]*>(.*?)<\\/\\1>").Value;

            // Get inner text from the tag.
            string innerText = Regex.Replace(firstTagElement, "<.*?>", string.Empty);

            // Set preview maxlength
            innerText = innerText.Length > previewMaxLength 
                ? innerText[.. previewMaxLength]
                : innerText;

            return $"{ innerText }...";
        }
    }
}
