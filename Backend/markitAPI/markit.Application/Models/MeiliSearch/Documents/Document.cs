namespace markit.Application.Models.MeiliSearch.Documents
{
    public class Document
    {
        public Document()
        {
            Id = Guid.NewGuid().ToString();
        }

        public string Id { get; }
    }
}
