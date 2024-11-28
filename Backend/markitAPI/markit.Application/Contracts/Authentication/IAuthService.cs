using markit.Application.Models.Authentication;
using markit.Application.Models.Authentication.Google;

namespace markit.Application.Contracts.Authentication
{
    public interface IAuthService
    {
        Task<AuthResponse> Login(AuthRequest request);
        Task<AuthResponse> LoginByGoogle(GoogleAuthRequest request);
    }
}
