using markit.Application.Models.Authentication;
using markit.Application.Models.Authentication.AppUser;
using Microsoft.AspNetCore.Http;

namespace markit.Application.Contracts.Authentication
{
    public interface IJwtService
    {
        Task<TokenModel> GenerateTokens(AppUser user);
        void SetInsideCookie(TokenModel tokenModel, HttpContext context);
        Task<TokenModel> Refresh(TokenModel tokens);
        Task Revoke(AppUser user);
    }
}
