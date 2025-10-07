using markit.Application.Models.Authentication;
using markit.Application.Models.Authentication.AppUser;
using markit.Application.Models.Authentication.Enums;
using Microsoft.AspNetCore.Authentication;

namespace markit.Application.Contracts.Authentication.ExternalLogin
{
    public interface IExternalLoginService
    {
        Task<List<ExternalSignInMethod>> GetByUser(AppUser user);
        AuthenticationProperties GetAuthenticationProperties(LoginProvider provider, LoginPurpose purpose, string redirectUrl, string? currentUserId);
        Task<string> LoginCallback(LoginProvider loginProvider);
        Task<string> LinkCallback(LoginProvider loginProvider);
        Task Remove(string userId, LoginProvider provider);
    }
}
