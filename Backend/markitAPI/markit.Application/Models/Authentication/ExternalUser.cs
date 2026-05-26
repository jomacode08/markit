namespace markit.Application.Models.Authentication
{
    public record ExternalUser(
        string Name,
        string Email,
        string? Picture = null
    ) {}
}
