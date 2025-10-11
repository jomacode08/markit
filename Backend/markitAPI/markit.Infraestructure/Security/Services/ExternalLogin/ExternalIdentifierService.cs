using System.Security.Claims;
using markit.Application.Contracts.Authentication.ExternalLogin;
using markit.Application.Models.Authentication.AppUser;
using markit.Application.Models.Authentication.Enums;
using Microsoft.AspNetCore.Identity;
using markit.Application.Common.Helpers;
using markit.Application.Contracts.Google;
using markit.Application.Contracts.GitHub;

namespace markit.Infraestructure.Security.Services.ExternalLogin
{
    public class ExternalIdentifierService : IExternalIdentifierService
    {
        private readonly IGoogleApiService _googleApiService;
        private readonly IGitHubApiService _gitHubApiService;
        private readonly UserManager<AppUser> _userManager;

        public ExternalIdentifierService(
            UserManager<AppUser> userManager,
            IGoogleApiService googleApiService,
            IGitHubApiService gitHubApiService
        )
        {
            _userManager = userManager;
            _googleApiService = googleApiService;
            _gitHubApiService = gitHubApiService;
        }

        public async Task<string> GetAsync(LoginProvider provider, AppUser user)
        {
            Claim? identifierClaim = await GetIdentifierClaim(provider, user);
            // Checking if the user has an identifier claim for the provider to return it.
            if (identifierClaim != null) return identifierClaim.Value;

            // Otherwise, get it from the provider.
            string identifier = provider switch
            {
                LoginProvider.Google => (await _googleApiService.GetUserProfileAsync(user)).Email,
                LoginProvider.GitHub => (await _gitHubApiService.GetProfileAsync(user)).UserName,
                _ => throw new InvalidOperationException($"Invalid or not implemented login provider: {provider.GetName()}")
            };

            await CreateIdentifierClaim(provider, user, identifier);
            return identifier;
        }

        public async Task Remove(LoginProvider provider, AppUser user)
        {
            Claim? identifierClaim = await GetIdentifierClaim(provider, user);

            if (identifierClaim != null)
            {
                await _userManager.RemoveClaimAsync(user, identifierClaim);
            }
        }

        private async Task<Claim?> GetIdentifierClaim(LoginProvider provider, AppUser user)
        {
            string type = GetClaimType(provider);
            return (await _userManager.GetClaimsAsync(user))
                .FirstOrDefault(c => c.Type.Equals(type));
        }

        private async Task CreateIdentifierClaim(LoginProvider provider, AppUser user, string identifier)
        {
            string type = GetClaimType(provider);
            await _userManager.AddClaimAsync(user, new Claim(type, identifier));
        }

        private static string GetClaimType(LoginProvider provider) => $"urn:{provider.GetName().ToLower()}:identifier";
    }
}
