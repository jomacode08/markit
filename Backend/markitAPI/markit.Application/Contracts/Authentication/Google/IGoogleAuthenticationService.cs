using markit.Application.Models.Authentication;

namespace markit.Application.Contracts.Authentication.Google
{
    public interface IGoogleAuthenticationService
    {
        Task<UserOperationModel> ValidateGoogleTokenId(string token);
    }
}
