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
        private readonly LoginProvider _provider;
        private readonly AppUser _user;

        public GitHubTokenStore(
            UserManager<AppUser> userManager,
            AppUser user
        )
        {
            _userManager = userManager;
            _provider = LoginProvider.GitHub;
            _user = user;
        }

        /// <summary>
        /// Retrieves a specific authentication token value for the user.
        /// </summary>
        /// <param name="key">The key identifying the token to retrieve.</param>
        /// <returns>The token value if found; otherwise, null.</returns>
        public async Task<string?> GetAsync(string key)
        {
            return await _userManager.GetAuthenticationTokenAsync(_user, _provider.GetName(), key);
        }

        /// <summary>
        /// Determines whether the current authorization has expired or is about to expire.
        /// </summary>
        /// <returns>
        /// True if the authorization is expired, missing, or will expire within 5 minutes; otherwise, false.
        /// </returns>
        public async Task<bool> IsAuthorizationExpiredAsync(string accessToken)
        {
            string? expiresAtStr = await GetAsync(EXPIRES_AT_TOKEN_NAME);
            if (string.IsNullOrEmpty(expiresAtStr) || string.IsNullOrEmpty(accessToken))
                throw new InvalidOperationException("No access token information found");

            if (DateTime.TryParse(expiresAtStr, out var expiresAt))
            {
                // Add 5 minutes buffer before expiration
                return DateTime.UtcNow >= expiresAt.AddMinutes(-5);
            }

            throw new FormatException("The stored experation token date has an invalid format");
        }

        /// <summary>
        /// Determines whether a given token has a valid GitHub API authorization.
        /// </summary>
        /// <param name="token">The GitHub access token to validate.</param>
        /// <param name="authSettings">The GitHub authentication settings.</param>
        /// <returns>
        /// True if the authorization is valid; false if unauthorized.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="token"/> or <paramref name="authSettings"/> is null or empty.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the validation process fails with an unexpected error.
        /// </exception>
        public async Task<bool> ValidateTokenAuthorization(string token, GitHubAuthSettings authSettings)
        {
            ArgumentNullException.ThrowIfNull(token, nameof(token));
            ArgumentNullException.ThrowIfNull(authSettings, nameof(authSettings));
            GitHubClient client = new(new ProductHeaderValue(authSettings.AppName))
            {
                Credentials = new Credentials(authSettings.ClientId, authSettings.ClientSecret)
            };

            try
            {
                ApplicationAuthorization? authorization = await client.Authorization.CheckApplicationAuthentication(
                    authSettings.ClientId,
                    accessToken: token
                );
                return authorization != null;
            }
            catch (ApiException ex)
            {
                throw new InvalidOperationException(
                    $"GitHub token validation failed with status code {ex.StatusCode}: {ex.Message}",
                    ex
                );
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException(
                    "GitHub token validation failed due to network error.",
                    ex
                );
            }
            catch (OperationCanceledException ex)
            {
                throw new InvalidOperationException(
                    "GitHub token validation request timed out.",
                    ex
                );
            }
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
            using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);
            await _userManager.RemoveAuthenticationTokenAsync(_user, _provider.GetName(), ACCESS_TOKEN_NAME);
            await _userManager.RemoveAuthenticationTokenAsync(_user, _provider.GetName(), REFRESH_TOKEN_NAME);
            await _userManager.RemoveAuthenticationTokenAsync(_user, _provider.GetName(), EXPIRES_AT_TOKEN_NAME);
            scope.Complete();
        }

        /// <summary>
        /// Removes all short-lived stored GitHub authentication tokens for the user.
        /// </summary>
        public async Task ClearShortLived()
        {
            using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);
            await _userManager.RemoveAuthenticationTokenAsync(_user, _provider.GetName(), ACCESS_TOKEN_NAME);
            await _userManager.RemoveAuthenticationTokenAsync(_user, _provider.GetName(), EXPIRES_AT_TOKEN_NAME);
            scope.Complete();
        }

        #region Helpers
        private async Task StoreAsync(string key, string value)
        {
            await _userManager.SetAuthenticationTokenAsync(_user, _provider.GetName(), key, value);
        }
        #endregion
    }
}
