using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.PeopleService.v1;
using Google.Apis.PeopleService.v1.Data;
using Google.Apis.Services;
using markit.Application.Contracts.Google;
using markit.Application.Models.Authentication.AppUser;
using markit.Application.Models.Authentication.Google;
using markit.Application.Models.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
namespace markit.Infraestructure.Security.Services.Google
{
    public class GoogleApiService : IGoogleApiService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly GoogleAuthSettings _googleAuthSettings;

        public GoogleApiService(UserManager<AppUser> userManager, IOptions<GoogleAuthSettings> googleAuthSettings)
        {
            _userManager = userManager;
            _googleAuthSettings = googleAuthSettings.Value;
        }

        public async Task<GoogleProfileData> GetUserProfile(AppUser user)
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