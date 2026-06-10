namespace markit.Application.Features.Notebooks.Queries.ViewModels
{
    public class NotebookSearchResult
    {
        public int Id { get; init; }
        public required string Name { get; init; }
        public IEnumerable<BlockSearchResult> Blocks { get; init; } = [];
    }

    public class BlockSearchResult
    {
        public int Id { get; init; }
        public required string Title { get; init; }
        // Snippet is mutable here for post-construction modification like output sanitization.
        public required string Snippet { get; set; }
        public int NotebookId { get; init; }
        public required string NotebookName { get; init; }
    }
}
