using markit.Application.Contracts.Persistence.Marks;
using markit.Domain.Entities;
using markit.Infraestructure.Persistence.EF;

namespace markit.Infraestructure.Repositorys.Marks
{
    public class BlockRepository : BaseRepository<Block>, IBlockRepository
    {
        public BlockRepository(MarkitDbContext mirefDbContext) : base(mirefDbContext)
        {
        }
    }
}
