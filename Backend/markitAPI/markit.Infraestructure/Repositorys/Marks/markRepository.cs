using System.Linq.Expressions;
using markit.Application.Contracts.Persistence.Marks;
using markit.Application.Features.Collections.Queries.ViewModels;
using markit.Domain.Entities;
using markit.Infraestructure.Persistence.EF;
using Microsoft.Data.SqlClient;
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

        public Task<List<Mark>> GetAsyncCursorBasedPagination(
            int pageSize,
            CursorData? cursor,
            SortPaginationOrder sortOrder = SortPaginationOrder.Ascending,
            int? collectionId = null,
            bool onlyFavorites = false
        )
        {
            IQueryable<Mark> marksQuery = context.Marks.AsNoTracking();

            // Apply filters
            if (collectionId.HasValue)
            {
                marksQuery = marksQuery.Where(m => m.CollectionId.Equals(collectionId));
            }

            if (onlyFavorites)
            {
                marksQuery = marksQuery.Where(m => m.IsFavorite);
            }

            if (cursor != null)
            {
                var cursorFilter = GetCursorBasedPaginationFilterExpression(sortOrder, cursor);
                marksQuery = marksQuery.Where(cursorFilter);
            }

            // Includes
            marksQuery = marksQuery.Include(m => m.Blocks);

            // Sorting
            if (sortOrder.Equals(SortPaginationOrder.Ascending))
            {
                marksQuery = marksQuery
                    .OrderBy(m => m.CreatedDate)
                    .ThenBy(m => m.Id);
            }
            else
            {
                marksQuery = marksQuery
                    .OrderByDescending(m => m.CreatedDate)
                    .ThenByDescending(m => m.Id);
            }

            return marksQuery
                .Take(pageSize + 1)
                .ToListAsync();
        }

        private static Expression<Func<Mark, bool>> GetCursorBasedPaginationFilterExpression(SortPaginationOrder sortOrder, CursorData cursor)
        {
            if (sortOrder.Equals(SortPaginationOrder.Ascending))
            {
                return m =>
                    m.CreatedDate > cursor.CreatedAt ||
                    (
                        m.CreatedDate == cursor.CreatedAt &&
                        (
                            cursor.Type != CollectionItemType.Mark ||
                            m.Id > cursor.Id
                        )
                    );

            }
            else
            {
                return m =>
                    m.CreatedDate < cursor.CreatedAt ||
                    (
                        m.CreatedDate == cursor.CreatedAt &&
                        (
                            cursor.Type != CollectionItemType.Mark ||
                            m.Id < cursor.Id
                        )
                    );
            }
        }
    }
}
