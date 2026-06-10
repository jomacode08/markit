using markit.Application.Contracts.Persistence.Notebooks;
using markit.Application.Features.Collections.Queries.ViewModels;
using markit.Application.Features.Notebooks.Queries.ViewModels;
using markit.Domain.Entities;
using markit.Infrastructure.Persistence.EF;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;
using System.Linq.Expressions;

namespace markit.Infrastructure.Repositories
{
    public class NotebookRepository : BaseRepository<Notebook>, INoteBookRepository
    {
        public NotebookRepository(MarkitDbContext markitDbContext) : base(markitDbContext)
        { }

        public async Task<Notebook?> GetWithOrderedBlocks(int id)
        {
            var notebook = await context.Notebooks
                .Include(m => m.Blocks)
                .Include(m => m.Collection)
                .Where(m => m.Id.Equals(id))
                .FirstOrDefaultAsync();

            if (notebook != null && notebook.Blocks != null)
            {
                notebook.Blocks = notebook.Blocks.OrderBy(b => b.Order).ToList();
            }

            return notebook;
        }

        public Task<List<Notebook>> GetAsyncCursorBasedPagination(
            int pageSize,
            string userId,
            CursorData? cursor,
            SortPaginationOrder sortOrder = SortPaginationOrder.Ascending,
            int? collectionId = null,
            bool onlyFavorites = false
        )
        {
            IQueryable<Notebook> notebooksQuery = context.Notebooks.AsNoTracking();

            // Apply filters
            notebooksQuery = notebooksQuery.Include(m => m.Collection)
                .Where(m => m.Collection != null && m.Collection.UserId.Equals(userId));

            if (collectionId.HasValue)
            {
                notebooksQuery = notebooksQuery.Where(m => m.CollectionId.Equals(collectionId));
            }

            if (onlyFavorites)
            {
                notebooksQuery = notebooksQuery.Where(m => m.IsFavorite);
            }

            if (cursor != null)
            {
                var cursorFilter = GetCursorBasedPaginationFilterExpression(sortOrder, cursor);
                notebooksQuery = notebooksQuery.Where(cursorFilter);
            }

            // Includes
            notebooksQuery = notebooksQuery.Include(m => m.Blocks);

            // Sorting
            if (sortOrder.Equals(SortPaginationOrder.Ascending))
            {
                notebooksQuery = notebooksQuery
                    .OrderBy(m => m.UpdatedDate)
                    .ThenBy(m => m.Id);
            }
            else
            {
                notebooksQuery = notebooksQuery
                    .OrderByDescending(m => m.UpdatedDate)
                    .ThenByDescending(m => m.Id);
            }

            return notebooksQuery
                .Take(pageSize + 1)
                .ToListAsync();
        }

        public async Task<List<Notebook>> GetMostRecentAsync(string userId, int limit)
        {
            return await context.Notebooks
                .AsNoTracking()
                .Include(m => m.Collection)
                .Where(m => m.Collection != null && m.Collection.UserId.Equals(userId))
                .OrderByDescending(m => m.CreatedDate)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<int> CountByUserIdAsync(string userId)
        {
            return await context.Notebooks
                .Where(m => m.Collection != null && m.Collection.UserId.Equals(userId))
                .CountAsync();
        }

        public async Task<IEnumerable<NotebookSearchResult>> SearchAsync(string searchTerm, string userId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(searchTerm)) return [];

            const string LANGUAGE_CONFIGURATION = "English";
            const string SEARCH_VECTOR_SHADOW_PROPERTY_NAME = "SearchVector";

            return await context.Notebooks
                // Query processing
                .Include(m => m.Collection)
                .Where(m => m.Collection != null && m.Collection.UserId == userId)
                .Where(m =>
                    EF.Property<NpgsqlTsVector>(m, SEARCH_VECTOR_SHADOW_PROPERTY_NAME)
                        .Matches(EF.Functions.WebSearchToTsQuery(LANGUAGE_CONFIGURATION, searchTerm))
                )
                // Rank implementation
                .OrderByDescending(m =>
                    EF.Property<NpgsqlTsVector>(m, SEARCH_VECTOR_SHADOW_PROPERTY_NAME)
                        .Rank(EF.Functions.WebSearchToTsQuery(LANGUAGE_CONFIGURATION, searchTerm))
                )
                .Take(10)
                // Query projection
                .Select(m => new NotebookSearchResult
                {
                    Id = m.Id,
                    Name = m.Name
                })
                .ToListAsync(cancellationToken);
        }

        private static Expression<Func<Notebook, bool>> GetCursorBasedPaginationFilterExpression(SortPaginationOrder sortOrder, CursorData cursor)
        {
            if (sortOrder.Equals(SortPaginationOrder.Ascending))
            {
                return m =>
                    m.CreatedDate > cursor.CreatedAt ||
                    
                        m.CreatedDate == cursor.CreatedAt &&
                        (
                            cursor.Type != CollectionItemType.Notebook ||
                            m.Id > cursor.Id
                        )
                    ;

            }
            else
            {
                return m =>
                    m.CreatedDate < cursor.CreatedAt ||
                    
                        m.CreatedDate == cursor.CreatedAt &&
                        (
                            cursor.Type != CollectionItemType.Notebook ||
                            m.Id < cursor.Id
                        )
                    ;
            }
        }
    }
}
