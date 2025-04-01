using markit.Application.Contracts.Persistence.Common;
using markit.Application.Contracts.Persistence.Marks;
using markit.Domain.Entities;
using markit.Infraestructure.Persistence.EF;

namespace markit.Infraestructure.Repositorys.Marks
{
    public class CollectionRepository : BaseRepository<Collection>, ICollectionRepository
    {
        public CollectionRepository(MarkitDbContext mirefDbContext) : base(mirefDbContext)
        {
        }
    }
}
