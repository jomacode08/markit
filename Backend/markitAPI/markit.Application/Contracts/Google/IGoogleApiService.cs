using markit.Application.Models.Authentication.AppUser;
using markit.Application.Models.Google;

namespace markit.Application.Contracts.Google
{
    public interface IGoogleApiService
    {
        Task<GoogleProfileData?> GetUserProfile(AppUser user);
    }
}
