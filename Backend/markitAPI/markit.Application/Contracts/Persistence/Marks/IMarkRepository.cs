using markit.Application.Contracts.Persistence.Common;
using markit.Application.Features.Collections.Queries.ViewModels;
using markit.Domain.Entities;

namespace markit.Application.Contracts.Persistence.Marks
{
    public interface IMarkRepository : IAsyncRepository<Mark>
    {
        Task<Mark?> GetWithOrderedBlocks(int id);
        Task<List<Mark>> GetAsyncCursorBasedPagination(
            int pageSize,
            CursorData? cursor,
            SortPaginationOrder sortOrder = SortPaginationOrder.Ascending,
            int? collectionId = null,
            bool onlyFavorites = false
        );
        Task<Mark> UpdateSyncModelAsync(int markId, string documentId);
    }
}
