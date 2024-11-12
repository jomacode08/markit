using markit.Application.Models.Autentication;
using markit.Application.Models.Authentication;
using markit.Application.Models.Authentication.Google;

namespace markit.Application.Contracts.Autentication
{
    public interface IAuthService
    {
        Task<AuthResponse> Login(AuthRequest request);
        Task<AuthResponse> LoginByGoogle(GoogleAuthRequest request);
        Task CreateIdentityUser(UserViewModel request, int creatorId);
    }
}
