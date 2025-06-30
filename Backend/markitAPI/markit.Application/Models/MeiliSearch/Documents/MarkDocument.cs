namespace markit.Application.Models.MeiliSearch.Documents
{
    public class MarkDocument(string name, int markId, int creatorId) : Document
    {
        public string Name { get; } = name;
        public int MarkId { get; } = markId;
        public int CreatorId { get; } = creatorId;
    }
}
