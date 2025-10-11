using markit.Application.Models.Authentication.AppUser;

namespace markit.Application.Contracts.Authentication.ExternalLogin.Common
{
    public interface IExternalProviderBaseOperation
    {
        Task<bool> RevokeAccessAsync(AppUser user);
        Task ClearShortLivedTokensAsync(AppUser user);
    }
}
