using markit.Application.Models.Authentication.AppUser;
using markit.Application.Models.Authentication.Enums;

namespace markit.Application.Contracts.Authentication.ExternalLogin
{
    public interface IExternalIdentifierService
    {
        Task<string> GetAsync(LoginProvider provider, AppUser user);
        Task Remove(LoginProvider provider, AppUser user);
    }
}
