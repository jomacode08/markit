using System.Transactions;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Util.Store;
using markit.Application.Common.Helpers;
using markit.Application.Models.Authentication.AppUser;
using markit.Application.Models.Authentication.Enums;
using Microsoft.AspNetCore.Identity;
using static markit.Application.Helpers.GeneralConstant.ExternalToken;

namespace markit.Infraestructure.Security.Services.Google
{
    /// <summary>
    /// Custom implementation class called by Google libraries.
    /// Provides an adapter between Google and ASP.UserTokens table.
    /// </summary>
    public class GoogleTokenStore : IDataStore
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly string _userId;
        private readonly string _providerName;


        public GoogleTokenStore(UserManager<AppUser> userManager, string userId)
        {
            _userManager = userManager;
            _userId = userId;
            _providerName = LoginProvider.Google.GetName();
        }

        /// <summary>
        /// Called by the Google library to store the token response.
        /// Deconstructs the TokenResponse and stores its parts.
        /// </summary>
        public async Task StoreAsync<T>(string key, T value)
        {
            AppUser? user = await _userManager.FindByIdAsync(_userId);
            if (user == null) return;

            if (value is TokenResponse token)
            {
                await _userManager.SetAuthenticationTokenAsync(user, _providerName, ACCESS_TOKEN_NAME, token.AccessToken);

                if (token.ExpiresInSeconds != null)
                {
                    DateTime expiresAt = DateTime.UtcNow.AddSeconds((double)token.ExpiresInSeconds);
                    await _userManager.SetAuthenticationTokenAsync(user, _providerName, EXPIRES_AT_TOKEN_NAME, expiresAt.ToString("O"));
                }

                // Google only provides a refresh token on the first consent, so it might be null on subsequent logins.
                if (!string.IsNullOrEmpty(token.RefreshToken))
                {
                    await _userManager.SetAuthenticationTokenAsync(user, _providerName, REFRESH_TOKEN_NAME, token.RefreshToken);
                }
            }
        }

        /// <summary>
        /// Called by the Google library to delete the stored tokens.
        /// </summary>
        public async Task DeleteAsync<T>(string key)
        {
            AppUser? user = await _userManager.FindByIdAsync(_userId);
            if (user == null) return;
            await _userManager.RemoveAuthenticationTokenAsync(user, _providerName, key);
        }

        /// <summary>
        /// Called by the Google library to retrieve the token response.
        /// Constructs a TokenResponse from its stored parts.
        /// </summary>
        public async Task<T?> GetAsync<T>(string key)
        {
            AppUser? user = await _userManager.FindByIdAsync(_userId);
            if (user == null) return default;

            string? accessToken = await _userManager.GetAuthenticationTokenAsync(user, _providerName, ACCESS_TOKEN_NAME);
            string? refreshToken = await _userManager.GetAuthenticationTokenAsync(user, _providerName, REFRESH_TOKEN_NAME);

            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
            {
                return default;
            }

            TokenResponse tokenResponse = new()
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };

            return (T)(object)tokenResponse;
        }

        /// <summary>
        /// Clears all tokens for the user.
        /// </summary>
        public async Task ClearAsync()
        {
            AppUser? user = await _userManager.FindByIdAsync(_userId);
            if (user == null) return;

            using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);
            await _userManager.RemoveAuthenticationTokenAsync(user, _providerName, ACCESS_TOKEN_NAME);
            await _userManager.RemoveAuthenticationTokenAsync(user, _providerName, REFRESH_TOKEN_NAME);
            await _userManager.RemoveAuthenticationTokenAsync(user, _providerName, EXPIRES_AT_TOKEN_NAME);
            scope.Complete();
        }

        /// <summary>
        /// Clears only short-lived tokens for the user.
        /// </summary>
        public async Task ClearShortLived(AppUser user)
        {
            using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);
            await _userManager.RemoveAuthenticationTokenAsync(user, _providerName, ACCESS_TOKEN_NAME);
            await _userManager.RemoveAuthenticationTokenAsync(user, _providerName, EXPIRES_AT_TOKEN_NAME);
            scope.Complete();
        }
    }
}