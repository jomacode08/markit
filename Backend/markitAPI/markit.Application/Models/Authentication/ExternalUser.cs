namespace markit.Application.Models.Authentication
{
    public record ExternalUser(
        string FirstName,
        string LastName,
        string Email,
        string? Picture = null
    ) {}
}
