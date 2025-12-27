using markit.Application.Features.Collections.Queries.ViewModels;

namespace markit.Application.Features.Collections.Queries.GetCollectionItemsPagedQuery
{
    public class GetCollectionItemsPagedQueryDto
    {
        public int PageSize { get; set; }
        public string? Cursor { get; set; }
        public SortPaginationOrder SortOrder { get; set; } = SortPaginationOrder.Ascending;
        public required CollectionItemPageFilters Filters { get; set; }
    }
}
