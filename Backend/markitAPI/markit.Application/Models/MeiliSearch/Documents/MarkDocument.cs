namespace markit.Application.Models.MeiliSearch.Documents
{
    public class MarkDocument(string name, int markId, int creatorId) : Document
    {
        public string Name { get; set; } = name;
        public int MarkId { get; set;  } = markId;
        public int CreatorId { get; set; } = creatorId;
    }
}
