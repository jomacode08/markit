using System.Net.Http.Headers;
using System.Text;
using markit.Application.Contracts.GitHub;
using markit.Application.Models.Authentication.AppUser;
using markit.Application.Models.Authentication.Enums;
using markit.Application.Models.Authentication.GitHub;
using markit.Application.Models.GitHub;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Octokit;
using Microsoft.Extensions.Logging;
using static markit.Application.Helpers.GeneralConstant.ExternalToken;
using Microsoft.AspNetCore.Http;

namespace markit.Infraestructure.Security.Services.GitHub
{
    public class GitHubApiService : IGitHubApiService
    {
        private readonly ILogger<GitHubApiService> _logger;
        private readonly GitHubAuthSettings _settings;
        private readonly UserManager<AppUser> _userManager;
        private readonly HttpClient _httpClient;
        
        private readonly LoginProvider githubProvider;
        private GitHubClient? _client;

        public GitHubApiService(
            IOptions<GitHubAuthSettings> settings,
            ILogger<GitHubApiService> logger,
            HttpClient httpClient,
            UserManager<AppUser> userManager)
        {
            _settings = settings.Value;
            _logger = logger;
            _userManager = userManager;
            _httpClient = httpClient;
            githubProvider = LoginProvider.GitHub;
            _httpClient.BaseAddress = new Uri("https://api.github.com");
        }

        public async Task<GitHubProfileData> GetProfileAsync(AppUser user)
        {
            GitHubClient client = await GetOrCreateClient(user);
            var githubUser = await client.User.Current();
            return new GitHubProfileData(
                UserName: githubUser.Login,
                Name: githubUser.Name,
                Email: githubUser.Email
            );
        }

        public async Task<bool> RevokeAccessAsync(AppUser user)
        {
            bool tokensRevoked = await RevokeTokensAsync(user);
            if (tokensRevoked)
            {
                GitHubTokenStore tokenStore = new(_userManager, user.Id);
                await tokenStore.ClearAsync();
            }
            return true;
        }

        public async Task ClearShortLivedTokensAsync(AppUser user)
        {
            GitHubTokenStore tokenStore = new(_userManager, user.Id);
            await tokenStore.ClearShortLived();
        }

        #region helpers

        private async Task<GitHubClient> GetOrCreateClient(AppUser user)
        {
            _client ??= await CreateOAuthClient(user);
            return _client;
        }

        private async Task<string> GetAndValidateAccessToken(string userId)
        {
            GitHubTokenStore tokenStore = new(_userManager, userId);
            string accessToken = await tokenStore.GetAsync(ACCESS_TOKEN_NAME)
                ?? throw new UnauthorizedAccessException("No access token found");

            if (await tokenStore.IsAuthorizationExpiredAsync())
            {
                accessToken = await tokenStore.RefreshAuthorizationAsync(_settings);
            }

            await tokenStore.ValidateTokenAuthorization(accessToken, _settings);
            return accessToken;
        }

        private async Task<GitHubClient> CreateOAuthClient(AppUser user)
        {
            var loginInfo = await _userManager.GetLoginsAsync(user);
            UserLoginInfo gitHubLogin = loginInfo.FirstOrDefault(l => l.LoginProvider == githubProvider.ToString())
                ?? throw new UnauthorizedAccessException("GitHub login not found for the user");
            string accessToken = await GetAndValidateAccessToken(user.Id);

            return new GitHubClient(new Octokit.ProductHeaderValue(_settings.AppName))
            {
                Credentials = new Credentials(accessToken, AuthenticationType.Oauth)
            };
        }

        private async Task<bool> RevokeTokensAsync(AppUser user)
        {
            string accessToken = await GetAndValidateAccessToken(user.Id);
            string requestUri = $"/applications/{_settings.ClientId}/grant";
            string appNameSanitized = _settings.AppName.Trim().Replace("-", string.Empty);

            // Construct the Basic Authentication header
            string credentials = $"{_settings.ClientId}:{_settings.ClientSecret}";
            string base64Credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(credentials));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);
            // Set the User-Agent header
            _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(appNameSanitized, "1.0"));
            // Set the Accept header for GitHub API
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));

            // Prepare the request
            var requestBody = new { access_token = accessToken };
            string jsonContent = JsonConvert.SerializeObject(requestBody);
            StringContent content = new(jsonContent, Encoding.UTF8, "application/json");
            HttpRequestMessage request = new(HttpMethod.Delete, requestUri)
            {
                Content = content
            };

            try
            {
                HttpResponseMessage response = await _httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    // Status code 200 OK indicates successful revocation.
                    return true;
                }
                else
                {
                    // Handle other potential non-success status codes.
                    string reason = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Token revocation failed. Status: {{status}}. Reason: {{reason}}", [response.StatusCode, reason]);
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw new BadHttpRequestException($"Token http revocation request failed: { ex.Message }");
            }
        }
        #endregion
    }
}
