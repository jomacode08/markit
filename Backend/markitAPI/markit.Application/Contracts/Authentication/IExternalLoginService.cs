using markit.Application.Models.Authentication;
using markit.Application.Models.Authentication.AppUser;
using markit.Application.Models.Authentication.Enums;
using Microsoft.AspNetCore.Authentication;

namespace markit.Application.Contracts.Authentication
{
    public interface IExternalLoginService
    {
        Task<List<ExternalSignInMethod>> GetExternalSignInMethods(AppUser user); 
        Task<string> LoginCallback(LoginProvider loginProvider);
        Task<string> LinkAccountCallback(LoginProvider loginProvider);
        AuthenticationProperties ConfigureAuthenticationProperties(LoginProvider provider, LoginPurpose purpose, string redirectUrl, string? currentUserId);
        Task RemoveExternalTokens(string userId);
        Task RemoveLogin(string userId, LoginProvider provider);
    }
}
