namespace markit.Application.Models.MeiliSearch.Search
{
    public record DocumentSearch(
        string Query,
        int Limit,
        int CreatorId
    );

    public record FormattedDocumentSearch(
        string Query,
        string[] AttributesToHighlight,
        int Limit,
        int CreatorId
    );
}
