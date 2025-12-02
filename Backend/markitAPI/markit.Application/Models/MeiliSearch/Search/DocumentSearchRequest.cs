namespace markit.Application.Models.MeiliSearch.Search
{
    public enum DocumentSearchFilter
    {
        All = 0,
        Collections = 1,
        Marks = 2,
    }

    public class DocumentSearchRequest
    {
        public string Query { get; set; } = string.Empty;
        public DocumentSearchFilter Filter { get; set; }
    }
}
