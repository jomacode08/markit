namespace markit.Application.Models.MeiliSearch.Documents
{
    public class CollectionDocument(string name, int collectionId, int creatorId) : Document
    {
        public string Name { get; set; } = name;
        public int CollectionId { get; set; } = collectionId;
        public int CreatorId { get; set; } = creatorId;
    }
}
