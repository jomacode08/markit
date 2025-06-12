namespace markit.Application.Features.Collections.Queries.ViewModels
{
    public class CollectionItem
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int CollectionId { get; set; }
        public CollectionItemType Type { get; set; }
        public int TypeId { get; set; }
        public string Preview {  get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdateAt { get; set; }
    }

    public enum CollectionItemType
    {
        Collection,
        Mark
    }
}
