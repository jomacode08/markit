using markit.Application.Models.Authentication.AppUser;
using markit.Application.Models.Authentication.Enums;
using markit.Application.Common.Helpers;
using Microsoft.AspNetCore.Identity;
using Octokit;
using markit.Application.Models.Authentication.GitHub;
using System.Transactions;
using static markit.Application.Helpers.GeneralConstant.ExternalToken;


namespace markit.Infraestructure.Security.Services.GitHub
{
    /// <summary>
    /// Manages GitHub authentication tokens for a specific user, providing functionality to store,
    /// retrieve, refresh, and validate OAuth tokens using ASP.NET Core Identity.
    /// </summary>
    public class GitHubTokenStore
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly string _userId;
        private readonly LoginProvider _provider;

        public GitHubTokenStore(
            UserManager<AppUser> userManager,
            string userId
        )
        {
            _userManager = userManager;
            _userId = userId;
            _provider = LoginProvider.GitHub;
        }

        /// <summary>
        /// Retrieves a specific authentication token value for the user.
        /// </summary>
        /// <param name="key">The key identifying the token to retrieve.</param>
        /// <returns>The token value if found; otherwise, null.</returns>
        public async Task<string?> GetAsync(string key)
        {
            AppUser user = await GetUserAsync();
            return await _userManager.GetAuthenticationTokenAsync(user, _provider.GetName(), key);
        }

        /// <summary>
        /// Determines whether the current authorization has expired or is about to expire.
        /// </summary>
        /// <returns>
        /// True if the authorization is expired, missing, or will expire within 5 minutes; otherwise, false.
        /// </returns>
        public async Task<bool> IsAuthorizationExpiredAsync()
        {
            string? accessToken = await GetAsync(ACCESS_TOKEN_NAME);
            string? expiresAtStr = await GetAsync(EXPIRES_AT_TOKEN_NAME);

            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(expiresAtStr)) return true;

            if (DateTime.TryParse(expiresAtStr, out var expiresAt))
            {
                // Add 5 minutes buffer before expiration
                return DateTime.UtcNow >= expiresAt.AddMinutes(-5);
            }

            return true;
        }

        /// <summary>
        /// Determines whether a given token has a valid GitHub API authorization.
        /// </summary>
        /// <returns>
        /// True if the authorization is valid.
        /// </returns>
        /// <exception cref="UnauthorizedAccessException">
        /// Thrown when the validation process fails.
        /// </exception>
        public async Task<bool> ValidateTokenAuthorization(string token, GitHubAuthSettings authSettings)
        {
            ArgumentNullException.ThrowIfNull(authSettings);
            GitHubClient client = new(new ProductHeaderValue(authSettings.AppName))
            {
                Credentials = new Credentials(authSettings.ClientId, authSettings.ClientSecret)
            };

            try
            {
                await client.Authorization.CheckApplicationAuthentication(authSettings.ClientId, token);
            }
            catch (ApiException)
            {
                throw new UnauthorizedAccessException("Bad GitHub credentials");
            }

            return true;
        }

        /// <summary>
        /// Refreshes the GitHub authorization tokens using the stored refresh token.
        /// </summary>
        /// <param name="authSettings">The GitHub authentication settings.</param>
        /// <returns>The new access token.</returns>
        /// <exception cref="ArgumentNullException">Thrown when authSettings is null.</exception>
        /// <exception cref="UnauthorizedAccessException">
        /// Thrown when no refresh or access token is available.
        /// </exception>
        public async Task<string> RefreshAuthorizationAsync(GitHubAuthSettings authSettings)
        {
            ArgumentNullException.ThrowIfNull(authSettings);
            string? refreshToken = await GetAsync(REFRESH_TOKEN_NAME);

            if (string.IsNullOrEmpty(refreshToken))
                throw new UnauthorizedAccessException("No refresh token available");

            GitHubClient client = new(new ProductHeaderValue(authSettings.AppName));
            OauthTokenRenewalRequest tokenRenewalRequest = new(
                authSettings.ClientId,
                authSettings.ClientSecret,
                refreshToken
            );
            OauthToken response = await client.Oauth.CreateAccessTokenFromRenewalToken(tokenRenewalRequest);
            
            if (!string.IsNullOrEmpty(response.Error))
            {
                throw new InvalidOperationException($"Failed GitHub token refresh: {response.Error} {response.ErrorDescription}");
            }

            DateTime expiresAt = DateTime.UtcNow.AddSeconds(response.ExpiresIn);
            await StoreAsync(ACCESS_TOKEN_NAME, response.AccessToken);
            await StoreAsync(REFRESH_TOKEN_NAME, response.RefreshToken);
            await StoreAsync(EXPIRES_AT_TOKEN_NAME, expiresAt.ToString("O"));

            return response.AccessToken;
        }

        /// <summary>
        /// Removes all stored GitHub authentication tokens for the user.
        /// </summary>
        public async Task ClearAsync()
        {
            AppUser user = await GetUserAsync();

            using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);
            await _userManager.RemoveAuthenticationTokenAsync(user, _provider.GetName(), ACCESS_TOKEN_NAME);
            await _userManager.RemoveAuthenticationTokenAsync(user, _provider.GetName(), REFRESH_TOKEN_NAME);
            await _userManager.RemoveAuthenticationTokenAsync(user, _provider.GetName(), EXPIRES_AT_TOKEN_NAME);
            scope.Complete();
        }


        /// <summary>
        /// Removes all short-lived stored GitHub authentication tokens for the user.
        /// </summary>
        public async Task ClearShortLived()
        {
            AppUser user = await GetUserAsync();

            using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);
            await _userManager.RemoveAuthenticationTokenAsync(user, _provider.GetName(), ACCESS_TOKEN_NAME);
            await _userManager.RemoveAuthenticationTokenAsync(user, _provider.GetName(), EXPIRES_AT_TOKEN_NAME);
            scope.Complete();
        }

        #region Helpers
        private async Task<AppUser> GetUserAsync()
        {
            return await _userManager.FindByIdAsync(_userId)
                ?? throw new Application.Exceptions.NotFoundException("Users", _userId);
        }

        private async Task StoreAsync(string key, string value)
        {
            AppUser user = await GetUserAsync();
            await _userManager.SetAuthenticationTokenAsync(user, _provider.GetName(), key, value);
        }
        #endregion
    }
}
