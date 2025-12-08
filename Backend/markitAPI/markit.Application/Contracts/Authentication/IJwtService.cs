using markit.Application.Models.Authentication.AppUser;
using Microsoft.AspNetCore.Http;

namespace markit.Application.Contracts.Authentication
{
    public interface IJwtService
    {
        Task<string> WriteToken(AppUser user);
        void SetTokenInsideCookie(string accessToken, HttpContext context);
    }
}
