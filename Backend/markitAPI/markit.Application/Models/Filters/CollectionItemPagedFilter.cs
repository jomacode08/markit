namespace markit.Application.Models.Filters
{
    public class CollectionItemPagedFilter
    {
        public int CollectionId { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public CollectionItemCategory CollectionItemCategory { get; set; }
    }

    public enum CollectionItemCategory
    {
        All,
        Collections,
        Marks
    }
}
