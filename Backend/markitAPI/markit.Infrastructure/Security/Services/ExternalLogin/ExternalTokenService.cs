using markit.Application.Contracts.Authentication.ExternalLogin;
using markit.Application.Contracts.Google;
using markit.Application.Models.Authentication.AppUser;
using markit.Application.Models.Authentication.Enums;
using markit.Application.Common.Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using markit.Application.Contracts.GitHub;
using static markit.Application.Helpers.GeneralConstant;

namespace markit.Infrastructure.Security.Services.ExternalLogin
{
    public class ExternalTokenService : IExternalTokenService
    {
        private readonly IGoogleApiService _googleApiService;
        private readonly IGitHubApiService _gitHubApiService;
        private readonly UserManager<AppUser> _userManager;

        public ExternalTokenService(
            IGoogleApiService googleApiService,
            IGitHubApiService gitHubApiService,
            UserManager<AppUser> userManager
        )
        {
            _googleApiService = googleApiService;
            _gitHubApiService = gitHubApiService;
            _userManager = userManager;
        }

        public async Task ClearShortLivedAsync(AppUser user)
        {
            await _googleApiService.ClearShortLivedTokensAsync(user);
        }

        public async Task StoreAsync(LoginProvider provider, AppUser user, IEnumerable<AuthenticationToken> tokens)
        {
            IEnumerable<AuthenticationToken> foundedTokens = ValidateTokens(provider, tokens);

            foreach (AuthenticationToken token in foundedTokens)
            {
                await _userManager.SetAuthenticationTokenAsync(
                    user,
                    provider.GetName(),
                    token.Name,
                    token.Value
                );
            }
        }

        public static IEnumerable<AuthenticationToken> ValidateTokens(LoginProvider provider, IEnumerable<AuthenticationToken> authenticationTokens)
        {
            string[] tokensToFind = [
                Token.ACCESS_TOKEN_NAME,
                Token.REFRESH_TOKEN_NAME,
                Token.EXPIRES_AT_TOKEN_NAME
            ];

            IEnumerable<AuthenticationToken> foundedTokens = authenticationTokens
                .Where(t => tokensToFind.Contains(t.Name));

            if (!foundedTokens.Any())
                throw new InvalidOperationException($"The tokens weren't provided by the login provider: {provider.GetName()}");

            return foundedTokens;
        }
    }
}
