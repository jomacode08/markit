using markit.Application.Contracts.Persistence.Common;
using markit.Application.Features.Collections.Queries.ViewModels;
using markit.Application.Features.Marks.Queries.ViewModels;
using markit.Domain.Entities;

namespace markit.Application.Contracts.Persistence.Marks
{
    public interface IMarkRepository : IAsyncRepository<Notebook>
    {
        Task<int> CountByUserIdAsync(string userId);
        Task<Notebook?> GetWithOrderedBlocks(int id);
        Task<List<Notebook>> GetAsyncCursorBasedPagination(
            int pageSize,
            string userId,
            CursorData? cursor,
            SortPaginationOrder sortOrder = SortPaginationOrder.Ascending,
            int? collectionId = null,
            bool onlyFavorites = false
        );
        Task<List<Notebook>> GetMostRecentAsync(string userId, int limit);
        Task<IEnumerable<MarkSearchResult>> SearchAsync(string query, string userId, CancellationToken cancellationToken);
    }
}
