using markit.Application.Models.Authentication;

namespace markit.Application.Contracts.Authentication
{
    public interface ILoginService
    {
        Task<AuthResponse> Login(AuthRequest request);
    }
}
