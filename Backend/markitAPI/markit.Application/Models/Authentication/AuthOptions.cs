namespace markit.Application.Models.Authentication
{
    public record AuthOptions(
        bool IsDemoModeAvailable,
        bool IsGoogleAvailable,
        bool IsGitHubAvailable
    );
}
