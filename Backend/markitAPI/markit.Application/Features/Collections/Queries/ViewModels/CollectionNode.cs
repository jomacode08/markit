namespace markit.Application.Features.Collections.Queries.ViewModels
{
    public class CollectionNode
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsMain { get; set; }
        public int? ParentId { get; set; }
        public string? Emoji { get; set; }
        public IEnumerable<CollectionNode> Children { get; set; } = [];
    }
}
