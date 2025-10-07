using markit.Application.Contracts.Authentication.ExternalLogin;
using markit.Application.Contracts.Google;
using markit.Application.Models.Authentication.AppUser;
using markit.Application.Models.Authentication.Enums;
using markit.Application.Common.Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using markit.Application.Exceptions;

namespace markit.Infraestructure.Security.Services.ExternalLogin
{
    public class ExternalTokenService : IExternalTokenService
    {
        private readonly IGoogleApiService _googleApiService;
        private readonly UserManager<AppUser> _userManager;

        public ExternalTokenService(IGoogleApiService googleApiService, UserManager<AppUser> userManager)
        {
            _googleApiService = googleApiService;
            _userManager = userManager;
        }

        public IEnumerable<AuthenticationToken> FilterOperative(LoginProvider provider, IEnumerable<AuthenticationToken> authenticationTokens)
        {
            string[] token_names_to_find = provider switch
            {
                LoginProvider.Google => ["access_token", "refresh_token"],
                _ => throw new InvalidOperationException($"Invalid or not implemented login provider: {provider.GetName()}")
            };

            IEnumerable<AuthenticationToken> foundedTokens = authenticationTokens
                .Where(t => token_names_to_find.Contains(t.Name));

            if (!foundedTokens.Any())
                throw new InvalidOperationException($"The tokens weren't provided by the login provider: {provider.GetName()}");

            return foundedTokens;
        }

        public async Task ClearShortLivedAsync(string userId)
        {
            AppUser user = await _userManager.FindByIdAsync(userId)
                ?? throw new NotFoundException("Users", userId);
            await _googleApiService.ClearShortLivedTokensAsync(user);
        }

        public async Task StoreAsync(LoginProvider provider, AppUser user, IEnumerable<AuthenticationToken> tokens)
        {
            foreach (AuthenticationToken token in tokens)
            {
                await _userManager.SetAuthenticationTokenAsync(
                    user,
                    provider.GetName(),
                    token.Name,
                    token.Value
                );
            }
        }
    }
}
