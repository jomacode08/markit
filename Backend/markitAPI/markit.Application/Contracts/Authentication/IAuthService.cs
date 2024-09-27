using markit.Application.Models.Autentication;

namespace markit.Application.Contracts.Autentication
{
    public interface IAuthService
    {
        Task<AuthResponse> Login(AuthRequest request);
    }
}
