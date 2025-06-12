using markit.Application.Contracts.Persistence.Marks;
using markit.Application.Features.Collections.Queries.ViewModels;
using markit.Domain.Entities;
using markit.Infraestructure.Persistence.EF;
using Microsoft.EntityFrameworkCore;

namespace markit.Infraestructure.Repositorys.Marks
{
    public class MarkRepository : BaseRepository<Mark>, IMarkRepository
    {
        public MarkRepository(MarkitDbContext markitDbContext) : base(markitDbContext)
        { }

        public async Task<Mark?> GetWithOrderedBlocks(int id)
        {
            var mark = await context.Marks
                .Include(m => m.Blocks)
                .Include(m => m.Collection)
                .Where(m => m.Id.Equals(id))
                .FirstOrDefaultAsync();

            if (mark != null && mark.Blocks != null)
            {
                mark.Blocks = mark.Blocks.OrderBy(b => b.Order).ToList();
            }

            return mark;
        }

        public Task<List<Mark>> GetAsyncCursorBasedPagination(int collectionId, int pageSize, CursorData? cursor)
        {
            IQueryable<Mark> marksQuery = context.Marks.AsNoTracking();
            marksQuery = marksQuery.Where(c => c.CollectionId.Equals(collectionId));

            if (cursor != null)
            {
                marksQuery = marksQuery.Where(c =>
                    c.CreatedDate > cursor.CreatedAt ||
                    (c.CreatedDate.Equals(cursor.CreatedAt) &&
                        (cursor.Type != CollectionItemType.Mark || c.Id > cursor.Id))
                );
            }

            return marksQuery
                .Include(c => c.Blocks)
                .OrderBy(c => c.CreatedDate)
                .ThenBy(c => c.Id)
                .Take(pageSize + 1)
                .ToListAsync();
        }
    }
}
