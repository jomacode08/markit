namespace markit.Application.Features.Gists.Queries.ViewModels
{
    public enum GistResponseStatus
    {
        Success, Failure
    }

    public class GistResponse
    {
        public GistResponseStatus Status { get; set; }
        public string? ErrorMessage { get; set; }
        public GistViewModel? Gist { get; set; }
    }
}
