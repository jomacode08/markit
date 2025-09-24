using System.Security.Claims;
using AutoMapper;
using markit.Application.Common.Helpers;
using markit.Application.Contracts.Authentication;
using markit.Application.Contracts.Google;
using markit.Application.Exceptions;
using markit.Application.Features.Creators.Commands.CreateCreator;
using markit.Application.Models.Authentication;
using markit.Application.Models.Authentication.AppUser;
using markit.Application.Models.Authentication.Enums;
using markit.Application.Models.Google;
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

        private const string ACCESS_TOKEN_NAME = "access_token";

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
                ExternalLoginInfo? loginInfo = await _signInManager.GetExternalLoginInfoAsync()
                    ?? throw new InvalidOperationException("Login information not found, external cookie is lost or expired");
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
                if (loginInfo.AuthenticationTokens == null)
                    throw new InvalidOperationException("The authentication tokens must be provided by the login provider");

                // Store external access token
                AuthenticationToken externalAccessToken = GetToken(ACCESS_TOKEN_NAME, loginInfo.AuthenticationTokens)
                    ?? throw new InvalidOperationException($"The authentication token: { ACCESS_TOKEN_NAME } must be provided by the login provider");
                await StoreExternalAccessToken(loginProvider, externalAccessToken, user);

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

        public async Task RemoveExternalTokens(string userId)
        {
            AppUser user = await _userManager.FindByIdAsync(userId)
                ?? throw new NotFoundException("Users", userId);
            
            await _userManager.RemoveAuthenticationTokenAsync(user, LoginProvider.Google.GetName(), ACCESS_TOKEN_NAME);
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

        private async Task StoreExternalAccessToken(LoginProvider loginProvider, AuthenticationToken token, AppUser user)
        {
            await _userManager.SetAuthenticationTokenAsync(
                user,
                loginProvider.GetName(),
                token.Name,
                token.Value
            );
        }

        private async Task<string> GetExternalIdentifier(LoginProvider provider, AppUser user)
        {
            if (provider.Equals(LoginProvider.Google))
            {
                GoogleProfileData? profileData = await _googleApiService.GetUserProfile(user);
                return profileData?.Email ?? "Not-found";
            }

            throw new InvalidOperationException($"Invalid or not implemented login provider: {provider.GetName()}");
        }

        #endregion

        #region Utilities
        private static AuthenticationToken? GetToken(string name, IEnumerable<AuthenticationToken> authenticationTokens)
        {
            return authenticationTokens.FirstOrDefault(t => t.Name.Equals(name));
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
