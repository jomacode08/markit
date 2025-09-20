using markit.Application.Models.Authentication;
using markit.Application.Models.Authentication.AppUser;

namespace markit.Application.Contracts.Authentication
{
    public interface IJwtService
    {
        Task<AuthResponse> GenerateAuthResponse(AppUser user);
    }
}
