using System.Linq.Expressions;
using markit.Application.Contracts.Persistence.Marks;
using markit.Application.Exceptions;
using markit.Application.Features.Collections.Queries.ViewModels;
using markit.Domain.Entities;
using markit.Infraestructure.Persistence.EF;
using Microsoft.EntityFrameworkCore;

namespace markit.Infraestructure.Repositorys.Marks
{
    public class CollectionRepository : BaseRepository<Collection>, ICollectionRepository
    {
        public CollectionRepository(MarkitDbContext markitDbContext) : base(markitDbContext)
        {
        }

        public async Task<List<Collection>> GetHierarchyRecursively(int rootCollectionId)
        {
			var collections = context
				.Collections
				.FromSql($@"
					WITH CollectionHierarchy AS (
						SELECT 
							c1.Id, c1.Name, c1.Path, c1.PathNames, c1.IsMain,
							c1.ParentId, c1.CreatorId, c1.IsFavorite, c1.DocumentId, c1.LastSync, c1.Emoji,
							c1.CreatedDate, c1.CreatedBy, c1.UpdatedDate, c1.UpdatedBy, c1.Enable
						FROM Collections as c1
						WHERE c1.Id = { rootCollectionId } AND Enable = 1

						UNION ALL

						SELECT
							c2.Id, c2.Name, c2.Path, c2.PathNames, c2.IsMain,
							c2.ParentId, c2.CreatorId, c2.IsFavorite, c2.DocumentId, c2.LastSync, c2.Emoji,
							c2.CreatedDate, c2.CreatedBy, c2.UpdatedDate, c2.UpdatedBy, c2.Enable
						FROM Collections AS c2
						INNER JOIN CollectionHierarchy ch ON c2.ParentId = ch.Id
						WHERE c2.Enable = 1
					)
					SELECT * FROM CollectionHierarchy;
                ").IgnoreQueryFilters();

            return await collections.ToListAsync();
        }

		public Task<List<Collection>> GetAsyncCursorBasedPagination(
			int pageSize,
			int creatorId,
			CursorData? cursor,
            SortPaginationOrder sortOrder = SortPaginationOrder.Ascending,
			int? collectionId = null,
			bool onlyFavorites = false
		)
        {
			IQueryable<Collection> collectionsQuery = context.Collections.AsNoTracking();

			// Apply filters
			collectionsQuery = collectionsQuery.Where(c => c.IsMain.Equals(false) && c.CreatorId.Equals(creatorId));

			if (collectionId.HasValue)
			{
				collectionsQuery = collectionsQuery.Where(c => c.ParentId.Equals(collectionId));
			}

			if (onlyFavorites)
			{
				collectionsQuery = collectionsQuery.Where(c => c.IsFavorite);
			}

			if (cursor != null)
			{
				var cursorFilter = GetCursorBasedPaginationFilterExpression(sortOrder, cursor);
				collectionsQuery = collectionsQuery.Where(cursorFilter);
			}

			// Includes
			collectionsQuery = collectionsQuery.Include(c => c.Marks);

			// Sorting
			if (sortOrder.Equals(SortPaginationOrder.Ascending))
			{
				collectionsQuery = collectionsQuery
					.OrderBy(c => c.CreatedDate)
					.ThenBy(c => c.Id);
            }
			else
			{
                collectionsQuery = collectionsQuery
					.OrderByDescending(c => c.CreatedDate)
					.ThenByDescending(c => c.Id);
            }

			return collectionsQuery
				.Take(pageSize + 1)
				.ToListAsync();
        }

		public async Task<int> CountByCreatorIdAsync(int creatorId)
		{
			return await context.Collections
				.Where(c => c.CreatorId.Equals(creatorId))
				.CountAsync();
		}

        public async Task<Collection> UpdateSyncModelAsync(int collectionId, string documentId)
		{
			var collection = await context.Collections
				.IgnoreQueryFilters()
				.FirstOrDefaultAsync(c => c.Id.Equals(collectionId))
				?? throw new NotFoundException("Collection", collectionId);

			collection.DocumentId = documentId;
			collection.LastSync = DateTime.Now;

			await UpdateAsync(collection);
			return collection;
		}

        private static Expression<Func<Collection, bool>> GetCursorBasedPaginationFilterExpression(SortPaginationOrder sortOrder, CursorData cursor)
		{
            if (sortOrder.Equals(SortPaginationOrder.Ascending))
            {
                return c =>
                    c.CreatedDate > cursor.CreatedAt ||
                    (
                        c.CreatedDate == cursor.CreatedAt &&
                        (
                            cursor.Type != CollectionItemType.Collection ||
                            c.Id > cursor.Id
                        )
                    );

            }
            else
            {
                return c =>
                    c.CreatedDate < cursor.CreatedAt ||
                    (
                        c.CreatedDate == cursor.CreatedAt &&
                        (
                            cursor.Type != CollectionItemType.Collection ||
                            c.Id < cursor.Id
                        )
                    );
            }
        }
    }
}
