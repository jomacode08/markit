using markit.Application.Models.Authentication;
using markit.Application.Models.Authentication.AppUser;
using Microsoft.AspNetCore.Http;

namespace markit.Application.Contracts.Authentication
{
    public interface IJwtService
    {
        Task<TokenModel> GenerateTokenPairAsync(AppUser user);
        Task<TokenModel> RefreshAsync(TokenModel tokens);
        Task RevokeAsync(AppUser user);
        Task IssueAccessTokenAsync(AppUser user, HttpContext context, DateTime expiresAt);
        void SetTokenPairInCookies(TokenModel tokenModel, HttpContext context);
    }
}
