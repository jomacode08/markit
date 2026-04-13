namespace markit.Application.Features.Gists.Queries.ViewModels
{
    public record GistViewModel(
        string Url,
        string Id,
        string Title,
        string Description,
        string Author,
        DateTime CreatedAt,
        List<GistFileViewModel>? Files
    );

    public record GistFileViewModel(
        string Id,
        string FileName,
        string Type,
        string Content,
        string RawUrl,
        string? Language,
        string? Html = null
    );
}
