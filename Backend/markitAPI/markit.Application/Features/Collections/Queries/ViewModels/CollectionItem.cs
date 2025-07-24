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
        public bool IsFavorite { get; set; }
        public string? Emoji { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public enum CollectionItemType
    {
        Collection,
        Mark
    }
}
