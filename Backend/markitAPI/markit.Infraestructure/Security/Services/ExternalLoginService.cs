using System.Security.Claims;
using System.Transactions;
using AutoMapper;
using markit.Application.Common.Helpers;
using markit.Application.Contracts.Authentication;
using markit.Application.Contracts.Google;
using markit.Application.Exceptions;
using markit.Application.Features.Creators.Commands.CreateCreator;
using markit.Application.Models.Authentication;
using markit.Application.Models.Authentication.AppUser;
using markit.Application.Models.Authentication.Enums;
using markit.Infraestructure.Security.Services.Google;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static markit.Application.Helpers.GeneralConstant;

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

        private readonly string spaRedirectUrl;

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

            spaRedirectUrl = $"{_spaSettings.BaseUrl}/auth/redirect";
        }

        #region Public
        public async Task<List<ExternalSignInMethod>> GetExternalSignInMethods(AppUser user)
        {
            LoginProvider[] providers = [LoginProvider.Google];
            List<ExternalSignInMethod> externalSignInMethods = [];
            IList<UserLoginInfo> logins = await _userManager.GetLoginsAsync(user);

            foreach (LoginProvider provider in providers)
            {
                bool configured = logins.Any(l => l.LoginProvider.Equals(provider.GetName()));
                string? identifier = configured ? await GetExternalIdentifier(provider, user) : default;
                externalSignInMethods.Add(
                    new ExternalSignInMethod(
                        provider,
                        identifier,
                        configured
                    )
                );
            }

            return externalSignInMethods;
        }

        public async Task<string> LoginCallback(LoginProvider provider)
        {
            try
            {
                ExternalLoginInfo loginInfo = await GetAndValidateLoginInfo();
                AppUser? user = await _userManager.FindByLoginAsync(loginInfo.LoginProvider, loginInfo.ProviderKey);
                if (loginInfo.AuthenticationTokens == null)
                    throw new InvalidOperationException("The authentication tokens must be provided by the login provider");

                // === Transaction start ===
                using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);
                if (user == null)
                {
                    // Create a new account.
                    AppUserRequest appUserRequest = MapAppUserRequest(loginInfo.Principal.Claims, provider);
                    await CreateCreator(appUserRequest);
                    // Link the user with the external login.
                    user = await _userManager.FindByEmailAsync(appUserRequest.Email)
                        ?? throw new InvalidOperationException($"Something went wrong with user creation and their email link: {appUserRequest.Email}");
                    await _userManager.AddLoginAsync(user, loginInfo);
                }
                await HandleProviderTokens(loginInfo.AuthenticationTokens, provider, user);
                var auth = await _authService.GenerateAuthResponse(user);
                scope.Complete();
                // === Transaction end === 

                return $"{spaRedirectUrl}?state=success&purpose=sign-in&token={auth.Token}&meiliToken={auth.MeiliSearchToken}";
            }
            catch (Exception ex)
            {
                return LogAndRedirectError(ex.Message, spaRedirectUrl);
            }
        }

        public async Task<string> LinkAccountCallback(LoginProvider provider)
        {
            try
            {
                ExternalLoginInfo loginInfo = await GetAndValidateLoginInfo();
                AuthenticationProperties properties = loginInfo.AuthenticationProperties
                    ?? throw new InvalidOperationException("The authentication properties were not supplied");

                if (properties.Items.TryGetValue(AuthenticationItems.CURRENT_USERID_KEY, out string? userId)
                    && userId != null)
                {
                    await ValidateExistentLogins(provider, loginInfo.ProviderKey, userId);
                    AppUser user = await GetUser(userId);
                    using (TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        // Link user with the new login info.
                        await _userManager.AddLoginAsync(user, loginInfo);
                        // Get and store provider tokens.
                        if (loginInfo.AuthenticationTokens == null)
                            throw new InvalidOperationException("The authentication tokens must be provided by the login provider.");
                        await HandleProviderTokens(loginInfo.AuthenticationTokens, provider, user);
                        scope.Complete();
                    }
                    return $"{spaRedirectUrl}?state=success&purpose=link";
                }

                throw new InvalidOperationException("Invalid or missed authentication properties.");
            }
            catch (Exception ex)
            {
                return LogAndRedirectError(ex.Message, spaRedirectUrl);
            }
        }

        public AuthenticationProperties ConfigureAuthenticationProperties(
            LoginProvider provider,
            LoginPurpose purpose,
            string redirectUrl,
            string? currentUserId
        )
        {
            AuthenticationProperties properties = _signInManager.ConfigureExternalAuthenticationProperties(provider.GetName(), redirectUrl);
            properties.Items.Add(AuthenticationItems.LOGIN_PURPOSE_KEY, purpose.GetName());
            properties.AllowRefresh = true;

            if (purpose.Equals(LoginPurpose.LinkAccount) && currentUserId != null)
            {
                properties.Items.Add(AuthenticationItems.CURRENT_USERID_KEY, currentUserId);
            }

            return properties;
        }

        public async Task RemoveExternalTokens(string userId)
        {
            AppUser user = await GetUser(userId);
            GoogleTokenStore tokenStore = new(_userManager, user.Id);
            await tokenStore.ClearShortLived(user);
        }

        public async Task RemoveLogin(string userId, LoginProvider provider)
        {
            AppUser? user = await GetUser(userId);
            UserLoginInfo loginToRemove = await ValidateLoginRevoke(user, provider);

            bool accessRevoked = provider switch
            {
                LoginProvider.Google => await _googleApiService.RevokeAccessAsync(user),
                _ => throw new InvalidOperationException($"Invalid or not implemented login provider: {provider.GetName()}")
            };

            if (!accessRevoked) throw new CustomValidationException("Revoke login failed: it wasn't possible to revoke provider access");

            try
            {
                using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);
                await RemoveIdentifierClaim(provider, user);
                await _userManager.RemoveLoginAsync(user, provider.GetName(), loginToRemove.ProviderKey);
                scope.Complete();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to update account with userId: {{ID}} settings. The external access has been removed. {{reason}}", [userId, ex.Message]);
                throw;
            }
        }


        #endregion

        #region Helpers
        private async Task CreateCreator(AppUserRequest request)
        {
            CreateCreatorCommand command = _mapper.Map<CreateCreatorCommand>(request);
            await _mediator.Send(command);
        }

        private async Task<ExternalLoginInfo> GetAndValidateLoginInfo()
        {
            return await _signInManager.GetExternalLoginInfoAsync()
                ?? throw new InvalidOperationException("Login information not found, external cookie is lost or expired");
        }

        private async Task<AppUser> GetUser(string userId)
        {
            return await _userManager.FindByIdAsync(userId)
                ?? throw new NotFoundException("Users", userId);
        }

        private async Task<UserLoginInfo> ValidateLoginRevoke(AppUser user, LoginProvider provider)
        {
            IList<UserLoginInfo> logins = await _userManager.GetLoginsAsync(user);
            if (!logins.Any()) throw new InvalidOperationException("The user doesn't have any external linked account");

            bool hasPassword = await _userManager.HasPasswordAsync(user);
            if (!hasPassword && logins.Count <= 1) throw new CustomValidationException("You need to set a password to delete your linked account.");

            return logins.FirstOrDefault(l => l.LoginProvider.Equals(provider.GetName()))
                ?? throw new InvalidOperationException($"There isn't a linked account for the {provider.GetName()} provider.");
        }

        private async Task ValidateExistentLogins(
            LoginProvider provider,
            string providerKey,
            string userId)
        {
            AppUser? userWithLogin = await _userManager.FindByLoginAsync(provider.GetName(), providerKey);
            if (userWithLogin != null && userWithLogin.Id.Equals(userId)) throw new CustomValidationException("The account is already linked.");
            if (userWithLogin != null && userWithLogin.Id != userId) throw new CustomValidationException("The account is already linked by another user.");
        }

        private async Task HandleProviderTokens(
            IEnumerable<AuthenticationToken> providerTokens,
            LoginProvider provider,
            AppUser user
        )
        {
            IEnumerable<AuthenticationToken> operativeTokens = GetOperativeTokens(provider, providerTokens);
            await StoreTokens(provider, user, operativeTokens);
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

        private static IEnumerable<AuthenticationToken> GetOperativeTokens(LoginProvider provider, IEnumerable<AuthenticationToken> authenticationTokens)
        {
            string[] token_names_to_find = provider switch
            {
                LoginProvider.Google => ["access_token", "refresh_token"],
                _ => throw new InvalidOperationException($"Invalid or not implemented login provider: {provider.GetName()}")
            };

            IEnumerable<AuthenticationToken> foundedTokens = authenticationTokens
                .Where(t => token_names_to_find.Contains(t.Name));

            if (!foundedTokens.Any())
                throw new InvalidOperationException($"The tokens weren't provided by the login provider: {provider.GetName()}");

            return foundedTokens;
        }

        private async Task<Claim?> GetIdentifierClaim(LoginProvider provider, AppUser user)
        {
            string type = $"urn:{provider.GetName().ToLower()}:identifier";
            return (await _userManager.GetClaimsAsync(user))
                .FirstOrDefault(c => c.Type.Equals(type));
        }

        private async Task CreateIdentifierClaim(LoginProvider provider, AppUser user, string identifier)
        {
            string type = $"urn:{provider.GetName().ToLower()}:identifier";
            await _userManager.AddClaimAsync(user, new Claim(type, identifier));
        }

        private async Task RemoveIdentifierClaim(LoginProvider provider, AppUser user)
        {
            Claim? identifierClaim = await GetIdentifierClaim(provider, user);

            if (identifierClaim != null)
            {
                await _userManager.RemoveClaimAsync(user, identifierClaim);
            }
        }

        private async Task<string> GetExternalIdentifier(LoginProvider provider, AppUser user)
        {
            Claim? identifierClaim = await GetIdentifierClaim(provider, user);
            // Checking if the user has an identifier claim for the provider to return it.
            if (identifierClaim != null) return identifierClaim.Value;

            // Otherwise, get it from the provider.
            string identifier = provider switch
            {
                LoginProvider.Google => (await _googleApiService.GetUserProfileAsync(user)).Email,
                _ => throw new InvalidOperationException($"Invalid or not implemented login provider: {provider.GetName()}")
            };

            await CreateIdentifierClaim(provider, user, identifier);
            return identifier;
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

        private string LogAndRedirectError(string message, string spaRedirectUrl)
        {
            _logger.LogError($"External login failed: {{Message}}", message);
            return $"{spaRedirectUrl}?state=failure&error={message}";
        }
        #endregion
    }
}
