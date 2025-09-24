using markit.Application.Models.Authentication;
using markit.Application.Models.Authentication.AppUser;
using markit.Application.Models.Authentication.Enums;
using Microsoft.AspNetCore.Authentication;

namespace markit.Application.Contracts.Authentication
{
    public interface IExternalLoginService
    {
        Task<string> Callback(LoginProvider loginProvider);
        AuthenticationProperties GetExternalAuthenticationProperties(LoginProvider provider, string redirectUrl);
        Task<List<ExternalSignInMethod>> GetExternalSignInMethods(AppUser user); 
        Task RemoveExternalTokens(string userId);
    }
}
