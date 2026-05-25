namespace markit.Application.Features.Collections.Queries.ViewModels
{
    public class CollectionItemPageRequest
    {
        public string UserId { get; set; } = string.Empty;
        public int PageSize { get; set; }
        public string? Cursor { get; set; }
        public SortPaginationOrder SortOrder { get; set; } = SortPaginationOrder.Ascending;
        public required CollectionItemPageFilters Filters { get; set; }
    }

    public record CollectionItemPage(string? NewCursor, List<CollectionItem> Items, bool HasNextPage);

    public record CollectionItemPageFilters(
        CollectionItemTypeFilter Type,
        int? CollectionId = null,
        bool OnlyFavorites = false
    );

    public record CursorData(DateTime CreatedAt, CollectionItemType Type, int Id);

    public enum CollectionItemTypeFilter
    {
        All,
        Collection,
        Mark
    }

    public enum SortPaginationOrder
    {
        Ascending,
        Descending
    }
}
