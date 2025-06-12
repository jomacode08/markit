using markit.Application.Contracts.Persistence.Marks;
using markit.Application.Features.Collections.Queries.ViewModels;
using markit.Domain.Entities;
using markit.Infraestructure.Persistence.EF;
using Microsoft.EntityFrameworkCore;

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
							c1.Id, c1.Name, c1.Path, c1.PathNames, c1.IsMain, c1.ParentId, c1.CreatorId,
							c1.CreatedDate, c1.CreatedBy, c1.UpdatedDate, c1.UpdatedBy, c1.Enable
						FROM Collections as c1
						WHERE c1.Id = { rootCollectionId } AND Enable = 1

						UNION ALL

						SELECT
							c2.Id, c2.Name, c2.Path, c2.PathNames, c2.IsMain, c2.ParentId, c2.CreatorId,
							c2.CreatedDate, c2.CreatedBy, c2.UpdatedDate, c2.UpdatedBy, c2.Enable
						FROM Collections AS c2
						INNER JOIN CollectionHierarchy ch ON c2.ParentId = ch.Id
						WHERE c2.Enable = 1
					)
					SELECT * FROM CollectionHierarchy;
                ").IgnoreQueryFilters();

            return await collections.ToListAsync();
        }

		public Task<List<Collection>> GetAsyncCursorBasedPagination(int parentCollectionId, int pageSize, CursorData? cursor)
		{
			IQueryable<Collection> collectionsQuery = context.Collections.AsNoTracking();
			collectionsQuery = collectionsQuery.Where(c => c.ParentId.Equals(parentCollectionId));

			if (cursor != null)
			{
				collectionsQuery = collectionsQuery.Where(c =>
					c.CreatedDate > cursor.CreatedAt ||
					(c.CreatedDate.Equals(cursor.CreatedAt) &&
						(cursor.Type != CollectionItemType.Collection || c.Id > cursor.Id))
				);
			}

			return collectionsQuery
				.Include(c => c.Marks)
				.OrderBy(c => c.CreatedDate)
				.ThenBy(c => c.Id)
				.Take(pageSize + 1)
				.ToListAsync();
        }
    }
}
