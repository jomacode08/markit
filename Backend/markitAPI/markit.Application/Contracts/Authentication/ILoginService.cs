using markit.Application.Models.Authentication;
using markit.Application.Models.Authentication.AppUser;
using Microsoft.AspNetCore.Http;

namespace markit.Application.Contracts.Authentication
{
    public interface ILoginService
    {
        Task<AppUser> Login(AuthRequest request, HttpContext context);
    }
}
