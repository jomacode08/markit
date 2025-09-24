using System.Net.Http.Headers;
using System.Text.Json;
using markit.Application.Common.Helpers;
using markit.Application.Contracts.Google;
using markit.Application.Models.Authentication.AppUser;
using markit.Application.Models.Authentication.Enums;
using markit.Application.Models.Google;
using Microsoft.AspNetCore.Identity;

namespace markit.Infraestructure.Security.Services.Google
{
    public class GoogleApiService : IGoogleApiService
    {
        private readonly UserManager<AppUser> _userManager;

        private const string ACCESS_TOKEN_NAME = "access_token";
        private static readonly JsonSerializerOptions s_writeOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public GoogleApiService(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<GoogleProfileData?> GetUserProfile(AppUser user)
        {
            const string USERINFO_ENDPOINT_URL = "https://www.googleapis.com/oauth2/v3/userinfo";
            HttpClient client = await AuthorizeCall(user);

            var response = await client.GetAsync(USERINFO_ENDPOINT_URL);
            response.EnsureSuccessStatusCode();
            string jsonDataString = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<GoogleProfileData>(jsonDataString, s_writeOptions);
        }

        private async Task<HttpClient> AuthorizeCall(AppUser user)
        {
            string accessToken = await _userManager.GetAuthenticationTokenAsync(user, LoginProvider.Google.GetName(), ACCESS_TOKEN_NAME)
                ?? throw new InvalidOperationException("Google account not linked or access token not found");

            HttpClient client = new();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            return client;
        }
    }
}
