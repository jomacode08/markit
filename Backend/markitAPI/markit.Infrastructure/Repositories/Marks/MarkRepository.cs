﻿using markit.Application.Contracts.Persistence.Marks;
using markit.Application.Exceptions;
using markit.Application.Features.Collections.Queries.ViewModels;
using markit.Application.Features.Marks.Queries.ViewModels;
using markit.Domain.Entities;
using markit.Infrastructure.Persistence.EF;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;
using System.Linq.Expressions;

namespace markit.Infrastructure.Repositories.Marks
{
    public class MarkRepository : BaseRepository<Notebook>, IMarkRepository
    {
        public MarkRepository(MarkitDbContext markitDbContext) : base(markitDbContext)
        { }

        public async Task<Notebook?> GetWithOrderedBlocks(int id)
        {
            var mark = await context.Notebooks
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

        public Task<List<Notebook>> GetAsyncCursorBasedPagination(
            int pageSize,
            string userId,
            CursorData? cursor,
            SortPaginationOrder sortOrder = SortPaginationOrder.Ascending,
            int? collectionId = null,
            bool onlyFavorites = false
        )
        {
            IQueryable<Notebook> marksQuery = context.Notebooks.AsNoTracking();

            // Apply filters
            marksQuery = marksQuery.Include(m => m.Collection)
                .Where(m => m.Collection != null && m.Collection.UserId.Equals(userId));

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
                    .OrderBy(m => m.UpdatedDate)
                    .ThenBy(m => m.Id);
            }
            else
            {
                marksQuery = marksQuery
                    .OrderByDescending(m => m.UpdatedDate)
                    .ThenByDescending(m => m.Id);
            }

            return marksQuery
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

        public async Task<IEnumerable<MarkSearchResult>> SearchAsync(string searchTerm, string userId, CancellationToken cancellationToken)
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
                .Select(m => new MarkSearchResult
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
