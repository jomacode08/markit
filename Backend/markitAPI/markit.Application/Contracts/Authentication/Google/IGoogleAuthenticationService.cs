using markit.Application.Models.Authentication;

namespace markit.Application.Contracts.Authentication.Google
{
    public interface IGoogleAuthenticationService
    {
        Task<UserViewModel> ValidateGoogleTokenId(string token);
    }
}
