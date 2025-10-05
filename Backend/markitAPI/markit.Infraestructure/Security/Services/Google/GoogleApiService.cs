using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.PeopleService.v1;
using Google.Apis.PeopleService.v1.Data;
using Google.Apis.Services;
using markit.Application.Common.Helpers;
using markit.Application.Contracts.Google;
using markit.Application.Models.Authentication.AppUser;
using markit.Application.Models.Authentication.Enums;
using markit.Application.Models.Authentication.Google;
using markit.Application.Models.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
namespace markit.Infraestructure.Security.Services.Google
{
    public class GoogleApiService : IGoogleApiService
    {
        private readonly ILogger<GoogleApiService> _logger;
        private readonly UserManager<AppUser> _userManager;
        private readonly GoogleAuthSettings _googleAuthSettings;
        private readonly HttpClient _httpClient;
        public GoogleApiService(
            ILogger<GoogleApiService> logger,
            IOptions<GoogleAuthSettings> googleAuthSettings,
            UserManager<AppUser> userManager,
            HttpClient httpClient)
        {
            _logger = logger;
            _googleAuthSettings = googleAuthSettings.Value;
            _userManager = userManager;
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://oauth2.googleapis.com");
        }

        public async Task<GoogleProfileData> GetUserProfileAsync(AppUser user)
        {
            UserCredential credential = await CreateUserCredentialAsync(user);

            PeopleServiceService peopleService = new(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "markit"
            });

            var request = peopleService.People.Get("people/me");
            request.PersonFields = "names,emailAddresses,photos";
            Person person = await request.ExecuteAsync();

            return new GoogleProfileData(
                person.Names?.FirstOrDefault()?.DisplayName ?? "not-found",
                person.EmailAddresses?.FirstOrDefault()?.Value ?? "not-found",
                person.Photos?.FirstOrDefault()?.Url
            );
        }

        public async Task<bool> RevokeAccessAsync(AppUser user)
        {
            bool tokensRevoked = await RevokeTokensAsync(user);
            if (tokensRevoked) {
                // Remove tokens from the store.
                GoogleTokenStore tokenStore = new(_userManager, user.Id);
                await tokenStore.ClearAsync();
            }
            return true;
        }

        private async Task<bool> RevokeTokensAsync(AppUser user)
        {
            // By revoking the google refresh_token, it will revoke all the access tokens too.
            const string TOKEN_NAME_TO_REVOKE = "refresh_token";

            string token = await _userManager.GetAuthenticationTokenAsync(
                user, 
                loginProvider: LoginProvider.Google.GetName(),
                tokenName: TOKEN_NAME_TO_REVOKE
            ) ?? throw new InvalidOperationException("It wasn't possible to retrieve a valid google token");

            FormUrlEncodedContent content = new([
                new KeyValuePair<string, string>("token", token)
            ]);

            try
            {
                HttpResponseMessage response = await _httpClient.PostAsync("revoke", content);
                if (response.IsSuccessStatusCode)
                {
                    // Status code 200 OK indicates successful revocation or that the token was not found.
                    return true;
                }
                else
                {
                    // Handle other potential non-success status codes (e.g., a 400 Bad Request, though rare for Google's specific revoke endpoint)
                    string reason = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Token revocation failed. Status: {{status}}. Reason: {{reason}}", [response.StatusCode, reason]);
                    return false;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task<UserCredential> CreateUserCredentialAsync(AppUser user)
        {
            GoogleTokenStore tokenStore = new(_userManager, user.Id);

            GoogleAuthorizationCodeFlow flow = new(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = _googleAuthSettings.ClientId,
                    ClientSecret = _googleAuthSettings.ClientSecret
                },
                DataStore = tokenStore
            });

            TokenResponse tokenResponse = await tokenStore.GetAsync<TokenResponse>(user.Id)
                ?? throw new InvalidOperationException("It wasn't possible to provide a valid google token-response");
            return new UserCredential(flow, user.Id, tokenResponse);
        }
    }
}