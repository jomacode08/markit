using System.Security.Claims;
using AutoMapper;
using markit.Application.Common.Helpers;
using markit.Application.Contracts.Authentication;
using markit.Application.Contracts.Google;
using markit.Application.Features.Creators.Commands.CreateCreator;
using markit.Application.Models.Authentication;
using markit.Application.Models.Authentication.AppUser;
using markit.Application.Models.Authentication.Enums;
using markit.Application.Models.Google;
using markit.Infraestructure.Security.Services.Google;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace markit.Infraestructure.Security.Services
{
    public class ExternalLoginService : IExternalLoginService
    {
        private readonly ILogger<ExternalLoginService> _logger;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IJwtService _authService;
        private readonly IGoogleApiService _googleApiService;
        private readonly SpaSettings _spaSettings;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;

        public ExternalLoginService
        (
            ILogger<ExternalLoginService> logger,
            IMediator mediator,
            IMapper mapper,
            IJwtService authService,
            IOptions<SpaSettings> spaSettings,
            IGoogleApiService googleApiService,
            SignInManager<AppUser> signInManager,
            UserManager<AppUser> userManager)
        {
            _logger = logger;
            _mediator = mediator;
            _mapper = mapper;
            _authService = authService;
            _spaSettings = spaSettings.Value;
            _googleApiService = googleApiService;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        #region Public
        public async Task<string> Callback(LoginProvider loginProvider)
        {
            string spaRedirectUrl = $"{_spaSettings.BaseUrl}/auth/redirect";
            try
            {
                ExternalLoginInfo loginInfo = await _signInManager.GetExternalLoginInfoAsync()
                    ?? throw new InvalidOperationException("Login information not found, external cookie is lost or expired");
                if (loginInfo.AuthenticationTokens == null)
                    throw new InvalidOperationException("The authentication tokens must be provided by the login provider");
                
                AppUser? user = await _userManager.FindByLoginAsync(loginInfo.LoginProvider, loginInfo.ProviderKey);
                if (user == null)
                {
                    // Create the new account.
                    AppUserRequest appUserRequest = MapAppUserRequest(loginInfo.Principal.Claims, loginProvider);
                    await CreateCreator(appUserRequest);

                    // Link the user with the external login.
                    user = await _userManager.FindByEmailAsync(appUserRequest.Email)
                        ?? throw new InvalidOperationException($"Something went wrong with user creation and their email link: {appUserRequest.Email}");
                    await _userManager.AddLoginAsync(user, loginInfo);
                }

                // Handling tokens
                var tokens = GetOperativeTokens(loginProvider, loginInfo.AuthenticationTokens);
                await StoreTokens(loginProvider, user, tokens);

                // Issue markit-specific jwt token to return it
                var auth = await _authService.GenerateAuthResponse(user);
                return $"{ spaRedirectUrl }?token={ auth.Token }&meiliToken={ auth.MeiliSearchToken }";
            }
            catch (Exception ex)
            {
                return LogAndRedirectError(ex.Message, spaRedirectUrl);
            }
        }

        public AuthenticationProperties GetExternalAuthenticationProperties(LoginProvider provider, string redirectUrl)
        {
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider.GetName(), redirectUrl);
            properties.AllowRefresh = true;
            return properties;
        }

        public async Task<List<ExternalSignInMethod>> GetExternalSignInMethods(AppUser user)
        {
            List<ExternalSignInMethod> externalSignInMethods = [];
            IList<UserLoginInfo> logins = await _userManager.GetLoginsAsync(user);

            foreach (UserLoginInfo login in logins)
            {
                LoginProvider loginProvider = Utilities.GetLoginProviderFromName(login.LoginProvider);
                string identifier = await GetExternalIdentifier(loginProvider, user);
                externalSignInMethods.Add(new ExternalSignInMethod(loginProvider, identifier));
            }

            return externalSignInMethods;
        }

        public async Task RemoveExternalTokens(AppUser user)
        {
            GoogleTokenStore tokenStore = new(_userManager, user.Id);
            await tokenStore.ClearForLogout(user);
        }

        #endregion

        #region Helpers
        private async Task CreateCreator(AppUserRequest request)
        {
            CreateCreatorCommand command = _mapper.Map<CreateCreatorCommand>(request);
            await _mediator.Send(command);
        }

        private string LogAndRedirectError(string message, string spaRedirectUrl)
        {
            _logger.LogError($"External login failed: {{Message}}", message);
            return $"{spaRedirectUrl}?error={message}";
        }

        private async Task StoreTokens(LoginProvider provider, AppUser user, IEnumerable<AuthenticationToken> tokens)
        {
            foreach (AuthenticationToken token in tokens) {
                await _userManager.SetAuthenticationTokenAsync(
                    user,
                    provider.GetName(),
                    token.Name,
                    token.Value
                );
            }
        }

        private async Task<string> GetExternalIdentifier(LoginProvider provider, AppUser user)
        {
            string claimType = $"urn:{provider.GetName().ToLower()}:identifier";
            IList<Claim>? claims = await _userManager.GetClaimsAsync(user);
            Claim? identifierClaim = claims.FirstOrDefault(c => c.Type == claimType);
         
            // Checking if the user has an identifier claim for the provider to return it.
            if (identifierClaim != null) return identifierClaim.Value;

            // Otherwise, get it from the provider.
            string identifier = provider switch
            {
                LoginProvider.Google => (await _googleApiService.GetUserProfile(user)).Email,
                _ => throw new InvalidOperationException(GetInvalidProviderMessage(provider))
            };

            // Store the identifier claim and return it.
            await _userManager.AddClaimAsync(user, new Claim(claimType, identifier));
            return identifier;
        }
        #endregion

        #region Utilities
        private static IEnumerable<AuthenticationToken> GetOperativeTokens(LoginProvider provider, IEnumerable<AuthenticationToken> authenticationTokens)
        {
            string[] token_names_to_find = provider switch
            {
                LoginProvider.Google => ["access_token", "refresh_token"],
                _ => throw new InvalidOperationException(GetInvalidProviderMessage(provider))
            };

            IEnumerable<AuthenticationToken> foundedTokens = authenticationTokens
                .Where(t => token_names_to_find.Contains(t.Name));

            if (!foundedTokens.Any())
                throw new InvalidOperationException($"The tokens weren't provided by the login provider: {provider.GetName()}");

            return foundedTokens;
        }

        private static AppUserRequest MapAppUserRequest(IEnumerable<Claim> claims, LoginProvider loginProvider)
        {
            switch (loginProvider)
            {
                case LoginProvider.Google:
                    return ParseGoogleClaimsToAppUserRequest(claims);
                default:
                    {
                        throw new InvalidOperationException($"Invalid login provider: {loginProvider.GetName()}");
                    }
            }
        }

        private static string GetInvalidProviderMessage(LoginProvider provider)
        {
            return $"Invalid or not implemented login provider: {provider.GetName()}";
        }

        private static AppUserRequest ParseGoogleClaimsToAppUserRequest(IEnumerable<Claim> claims)
        {
            string email = (claims.FirstOrDefault(c => c.Type.Equals("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"))?.Value)
            ?? throw new InvalidOperationException("The required 'emailaddress' claim was not provided by the external google login provider.");

            string givenName = (claims.FirstOrDefault(c => c.Type.Equals("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname"))?.Value)
            ?? throw new InvalidOperationException("The required 'givenname' claim was not provided by the external google login provider.");

            string surName = (claims.FirstOrDefault(c => c.Type.Equals("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname"))?.Value)
            ?? throw new InvalidOperationException("The required 'surname' claim was not provided by the external google login provider.");

            string? picture = (claims.FirstOrDefault(c => c.Type.Equals("urn:google:picture"))?.Value);

            return new AppUserRequest(
                email,
                password: null,
                givenName,
                surName,
                picture,
                AccessType.External
            );
        }
        #endregion
    }
}
