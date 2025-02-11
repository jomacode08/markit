using markit.Application.Contracts.Persistence.Marks;
using markit.Domain.Entities;
using markit.Infraestructure.Persistence.EF;
using Microsoft.EntityFrameworkCore;

namespace markit.Infraestructure.Repositorys.Marks
{
    public class MarkRepository : BaseRepository<Mark>, IMarkRepository
    {
        public MarkRepository(MarkitDbContext markitDbContext) : base(markitDbContext)
        {
        }

        public async Task<Mark?> GetWithOrderedBlocks(int id)
        {
            var mark = await context.Marks
                .Include(m => m.Blocks)
                .Where(m => m.Id.Equals(id))
                .FirstOrDefaultAsync();

            if (mark != null && mark.Blocks != null)
            {
                mark.Blocks = mark.Blocks.OrderBy(b => b.Order).ToList();
            }

            return mark;
        }
    }
}
