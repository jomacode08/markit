using System.Linq.Expressions;
using markit.Application.Contracts.Persistence.Notebooks;
using markit.Application.Exceptions;
using markit.Application.Features.Collections.Queries.ViewModels;
using markit.Domain.Entities;
using markit.Infrastructure.Persistence.EF;
using Microsoft.EntityFrameworkCore;

namespace markit.Infrastructure.Repositories
{
    public class CollectionRepository : BaseRepository<Collection>, ICollectionRepository
    {
        private const int MaxDepth = 25;
        private const int MaxHierarchyNodes = 500;

        public CollectionRepository(MarkitDbContext markitDbContext) : base(markitDbContext)
        {}

        public async Task<List<Collection>> GetHierarchyRecursively(int rootCollectionId)
        {
			var collections = context
				.Collections
				.FromSql($@"
					WITH RECURSIVE collection_hierarchy AS (
						SELECT
							c1.id, c1.name, c1.path, c1.path_names, c1.is_main,
							c1.parent_id, c1.user_id, c1.is_favorite, c1.emoji,
							c1.created_date, c1.created_by, c1.updated_date,
							c1.updated_by, c1.enable, c1.description,
							0 AS depth
						FROM collections AS c1
						WHERE c1.id = {rootCollectionId} AND c1.enable = true

						UNION ALL

						SELECT
							c2.id, c2.name, c2.path, c2.path_names, c2.is_main,
							c2.parent_id, c2.user_id, c2.is_favorite, c2.emoji,
							c2.created_date, c2.created_by, c2.updated_date,
							c2.updated_by, c2.enable, c2.description,
							ch.depth + 1
						FROM collections AS c2
						INNER JOIN collection_hierarchy ch ON c2.parent_id = ch.id
						WHERE c2.enable = true AND ch.depth < {MaxDepth}
					)
					CYCLE id SET is_cycle USING cycle_path
					SELECT
						id, name, path, path_names, is_main,
						parent_id, user_id, is_favorite, emoji,
						created_date, created_by, updated_date,
						updated_by, enable, description
					FROM collection_hierarchy
					WHERE NOT is_cycle;
                ").IgnoreQueryFilters();

            List<Collection> result = await collections.ToListAsync();

            if (result.Count > MaxHierarchyNodes)
                throw new CustomValidationException(
                    $"The collection hierarchy exceeds the maximum allowed size of {MaxHierarchyNodes} nodes.");

            return result;
        }

		public Task<List<Collection>> GetAsyncCursorBasedPagination(
			int pageSize,
			string userId,
			CursorData? cursor,
            SortPaginationOrder sortOrder = SortPaginationOrder.Ascending,
			int? collectionId = null,
			bool onlyFavorites = false
		)
        {
			IQueryable<Collection> collectionsQuery = context.Collections.AsNoTracking();

			// Apply filters
			collectionsQuery = collectionsQuery.Where(c => c.IsMain.Equals(false) && c.UserId.Equals(userId));

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
			collectionsQuery = collectionsQuery.Include(c => c.Notebooks);

			// Sorting
			if (sortOrder.Equals(SortPaginationOrder.Ascending))
			{
				collectionsQuery = collectionsQuery
					.OrderBy(c => c.UpdatedDate)
					.ThenBy(c => c.Id);
            }
			else
			{
                collectionsQuery = collectionsQuery
					.OrderByDescending(c => c.UpdatedDate)
					.ThenByDescending(c => c.Id);
            }

			return collectionsQuery
				.Take(pageSize + 1)
				.ToListAsync();
        }

		public async Task<int> CountByUserIdAsync(string userId)
		{
			return await context.Collections
				.Where(c => c.UserId.Equals(userId))
				.CountAsync();
		}

        private static Expression<Func<Collection, bool>> GetCursorBasedPaginationFilterExpression(SortPaginationOrder sortOrder, CursorData cursor)
		{
            if (sortOrder.Equals(SortPaginationOrder.Ascending))
            {
                return c =>
                    c.CreatedDate > cursor.CreatedAt ||
                    
                        c.CreatedDate == cursor.CreatedAt &&
                        (
                            cursor.Type != CollectionItemType.Collection ||
                            c.Id > cursor.Id
                        )
                    ;

            }
            else
            {
                return c =>
                    c.CreatedDate < cursor.CreatedAt ||
                    
                        c.CreatedDate == cursor.CreatedAt &&
                        (
                            cursor.Type != CollectionItemType.Collection ||
                            c.Id < cursor.Id
                        )
                    ;
            }
        }
    }
}
