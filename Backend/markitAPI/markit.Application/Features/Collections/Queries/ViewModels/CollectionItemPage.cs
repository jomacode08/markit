namespace markit.Application.Features.Collections.Queries.ViewModels
{
    public record CollectionItemPage(string? NewCursor, List<CollectionItem> Items, bool HasNextPage);

    public record CollectionItemPageRequest(int CollectionId, int PageSize, CollectionItemFilter Filter, string? Cursor);

    public record CursorData(DateTime CreatedAt, CollectionItemType Type, int Id);

    public enum CollectionItemFilter
    {
        All,
        Collection,
        Mark
    }
}
