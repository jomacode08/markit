namespace markit.Application.Features.Collections.Queries.ViewModels
{
    public class CollectionViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsMain { get; set; }
        public int? ParentId { get; set; }
        public int CreatorId { get; set; }

        public List<CollectionPath>? Path { get; set; }
        public List<CollectionItem>? CollectionItems { get; set; }
    }

    public class CollectionPath
    {
        public int CollectionId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
