using markit.Application.Models.Authentication.AppUser;
using markit.Application.Models.Google;

namespace markit.Application.Contracts.Google
{
    public interface IGoogleApiService
    {
        Task<GoogleProfileData> GetUserProfileAsync(AppUser user);
        Task<bool> RevokeAccessAsync(AppUser user);
        Task ClearShortLivedTokensAsync(AppUser user);
    }
}
