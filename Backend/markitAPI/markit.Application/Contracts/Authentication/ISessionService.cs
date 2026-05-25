namespace markit.Application.Contracts.Authentication
{
    public interface ISessionService
    {
        string? GetIdentity();
        string GetUserId();
    }
}
