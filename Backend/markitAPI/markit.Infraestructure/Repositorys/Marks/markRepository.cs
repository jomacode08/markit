using markit.Application.Contracts.Persistence.Marks;
using markit.Domain.Entities;
using markit.Infraestructure.Persistence.EF;

namespace markit.Infraestructure.Repositorys.Marks
{
    public class MarkRepository : BaseRepository<Mark>, IMarkRepository
    {
        public MarkRepository(MarkitDbContext markitDbContext) : base(markitDbContext)
        {
        }
    }
}
