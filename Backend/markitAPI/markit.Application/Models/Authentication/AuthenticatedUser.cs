namespace markit.Application.Models.Authentication
{
    public record AuthenticatedUser(
        string UserId,
        string GivenName,
        string Email,
        IReadOnlyList<string> Roles,
        string? Picture
    );
}
