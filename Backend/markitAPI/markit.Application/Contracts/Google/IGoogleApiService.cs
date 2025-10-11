using markit.Application.Contracts.Authentication.ExternalLogin.Common;
using markit.Application.Models.Authentication.AppUser;
using markit.Application.Models.Google;

namespace markit.Application.Contracts.Google
{
    public interface IGoogleApiService : IExternalProviderBaseOperation
    {
        Task<GoogleProfileData> GetUserProfileAsync(AppUser user);
    }
}
