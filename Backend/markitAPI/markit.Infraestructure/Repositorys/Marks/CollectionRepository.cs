using markit.Application.Contracts.Persistence.Marks;
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
	                        Id, Name, Path, IsMain, ParentId, CreatorId,
	                        CreatedDate, CreatedBy, UpdatedDate, UpdatedBy, Enable
	                    FROM Collections
	                    WHERE Id = { rootCollectionId } AND Enable = 1

	                    UNION ALL

	                    SELECT
	                        c.Id, c.Name, c.Path, c.IsMain, c.ParentId, c.CreatorId,
	                        c.CreatedDate, c.CreatedBy, c.UpdatedDate, c.UpdatedBy, c.Enable
	                    FROM Collections AS c
	                    INNER JOIN CollectionHierarchy ch ON c.ParentId = ch.Id
	                    WHERE c.Enable = 1
                    )
                    SELECT * FROM CollectionHierarchy;
                ").IgnoreQueryFilters();

            return await collections.ToListAsync();
        }
    }
}
