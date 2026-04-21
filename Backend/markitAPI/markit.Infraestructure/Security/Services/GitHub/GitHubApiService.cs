using System.Net.Http.Headers;
using System.Text;
using markit.Application.Contracts.GitHub;
using markit.Application.Models.Authentication.AppUser;
using markit.Application.Models.Authentication.GitHub;
using markit.Application.Models.GitHub;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Octokit;
using Microsoft.Extensions.Logging;
using static markit.Application.Helpers.GeneralConstant.Token;
using Microsoft.AspNetCore.Http;
using markit.Application.Features.Gists.Queries.ViewModels;
using markit.Application.Common.Helpers;

namespace markit.Infraestructure.Security.Services.GitHub
{
    public class GitHubApiService : IGitHubApiService
    {
        private readonly ILogger<GitHubApiService> _logger;
        private readonly GitHubAuthSettings _settings;
        private readonly UserManager<AppUser> _userManager;
        private readonly HttpClient _httpClient;
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
            _httpClient.BaseAddress = new Uri("https://api.github.com");
        }

        public async Task<bool> RevokeAccessAsync(AppUser user)
        {
            bool tokensRevoked = await RevokeTokensAsync(user);
            if (tokensRevoked)
            {
                GitHubTokenStore tokenStore = new(_userManager, user);
                await tokenStore.ClearAsync();
                return true;
            }
            return false;
        }

        public async Task ClearShortLivedTokensAsync(AppUser user)
        {
            GitHubTokenStore tokenStore = new(_userManager, user);
            await tokenStore.ClearShortLived();
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

        public async Task<GistViewModel> GetGistById(string gistId, AppUser user)
        {
            GitHubClient client = await GetOrCreateClient(user);
            Gist gist = await client.Gist.Get(gistId);
            const string MARKDOWN_FILE_TYPE = "text/markdown";

            List<GistFileViewModel> gistFilesVm = new(gist.Files.Count);
            foreach (GistFile file in gist.Files.Values) {
                gistFilesVm.Add(new GistFileViewModel(
                    Id: Guid.NewGuid().ToString(),
                    file.Filename,
                    file.Type,
                    file.Content,
                    file.RawUrl,
                    file.Language,
                    Html: MARKDOWN_FILE_TYPE.Equals(file.Type, StringComparison.OrdinalIgnoreCase)
                        ? Utilities.SanitizeHtml(Utilities.ConvertMarkdownToHtml(file.Content))
                        : null
                ));
            }

            string title = gistFilesVm.Count != 0
                ? gistFilesVm[0].FileName
                : $"gist:{gist.Id}";

            return new GistViewModel(
                Url: gist.HtmlUrl,
                Id: gist.Id,
                Title: title,
                Description: gist.Description,
                Author: gist.Owner.Login,
                CreatedAt: gist.CreatedAt.DateTime,
                Files: gistFilesVm
            );
        }

        #region helpers

        private async Task<GitHubClient> GetOrCreateClient(AppUser user)
        {
            _client ??= await CreateOAuthClient(user);
            return _client;
        }

        private async Task<string> GetAndValidateAccessToken(AppUser user)
        {
            GitHubTokenStore tokenStore = new(_userManager, user);
            string accessToken = await tokenStore.GetAsync(ACCESS_TOKEN_NAME)
                ?? throw new UnauthorizedAccessException("No access token found");

            if (await tokenStore.IsAuthorizationExpiredAsync(accessToken))
            {
                accessToken = await tokenStore.RefreshAuthorizationAsync(_settings);
            }

            return await tokenStore.ValidateTokenAuthorization(accessToken, _settings)
                ? accessToken 
                : throw new InvalidOperationException("Invalid or unauthorized GitHub access token.");
        }

        private async Task<GitHubClient> CreateOAuthClient(AppUser user)
        {
            string accessToken = await GetAndValidateAccessToken(user);
            return new GitHubClient(new Octokit.ProductHeaderValue(_settings.AppName))
            {
                Credentials = new Credentials(accessToken, AuthenticationType.Oauth)
            };
        }

        private async Task<bool> RevokeTokensAsync(AppUser user)
        {
            GitHubTokenStore tokenStore = new(_userManager, user);
            string accessToken = await tokenStore.GetAsync(ACCESS_TOKEN_NAME)
                ?? await tokenStore.RefreshAuthorizationAsync(_settings);

            string requestUri = $"/applications/{_settings.ClientId}/grant";
            string appNameSanitized = _settings.AppName.Trim().Replace("-", string.Empty);

            // Prepare the request
            var requestBody = new { access_token = accessToken };
            string jsonContent = JsonConvert.SerializeObject(requestBody);
            string credentials = $"{_settings.ClientId}:{_settings.ClientSecret}";
            string base64Credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(credentials));
            StringContent content = new(jsonContent, Encoding.UTF8, "application/json");
            
            HttpRequestMessage request = new(HttpMethod.Delete, requestUri)
            {
                Content = content
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);
            request.Headers.UserAgent.Add(new ProductInfoHeaderValue(appNameSanitized, "1.0"));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));

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
                    _logger.LogError("Token revocation failed. Status: {Status}. Reason: {Reason}", response.StatusCode, reason);
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
