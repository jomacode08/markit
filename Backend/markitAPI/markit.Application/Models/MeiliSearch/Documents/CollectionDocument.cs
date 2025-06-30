namespace markit.Application.Models.MeiliSearch.Documents
{
    public class CollectionDocument(string name, int collectionId, int creatorId) : Document
    {
        public string Name { get; } = name;
        public int CollectionId { get; } = collectionId;
        public int CreatorId { get; } = creatorId;
    }
}
