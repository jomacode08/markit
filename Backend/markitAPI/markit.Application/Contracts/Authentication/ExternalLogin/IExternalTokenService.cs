using markit.Application.Models.Authentication.AppUser;
using markit.Application.Models.Authentication.Enums;
using Microsoft.AspNetCore.Authentication;

namespace markit.Application.Contracts.Authentication.ExternalLogin
{
    public interface IExternalTokenService
    {
        Task StoreAsync(LoginProvider provider, AppUser user, IEnumerable<AuthenticationToken> tokens);
        Task ClearShortLivedAsync(string userId);
    }
}
