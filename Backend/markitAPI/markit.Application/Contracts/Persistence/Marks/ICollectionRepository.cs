using markit.Application.Contracts.Persistence.Common;
using markit.Application.Features.Collections.Queries.ViewModels;
using markit.Domain.Entities;

namespace markit.Application.Contracts.Persistence.Marks
{
    public interface ICollectionRepository : IAsyncRepository<Collection>
    {
        Task<List<Collection>> GetHierarchyRecursively(int rootCollectionId);
        Task<List<Collection>> GetAsyncCursorBasedPagination(
            int pageSize,
            int creatorId,
            CursorData? cursor,
            SortPaginationOrder sortOrder = SortPaginationOrder.Ascending,
            int? collectionId = null,
            bool onlyFavorites = false
        );
        Task<int> CountByCreatorIdAsync(int creatorId);
    }
}
