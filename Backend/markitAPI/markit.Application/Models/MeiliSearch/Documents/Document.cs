namespace markit.Application.Models.MeiliSearch.Documents
{
    public class Document
    {
        public Document()
        {
            Id = Guid.NewGuid().ToString();
            Enabled = true;
        }

        public string Id { get; set; }
        public bool Enabled { get; set; }
    }
}
