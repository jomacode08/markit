using markit.Application.Models.Authentication;
using markit.Application.Models.Authentication.Google;


namespace markit.Application.Contracts.Authentication.Google
{
    public interface IGoogleAuthenticationService
    {
        Task<GoogleAuthResponse> ValidateGoogleTokenId(string token);
    }
}
