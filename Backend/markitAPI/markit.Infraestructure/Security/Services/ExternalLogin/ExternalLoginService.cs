using System.Security.Claims;
using System.Transactions;
using AutoMapper;
using markit.Application.Common.Helpers;
using markit.Application.Contracts.Authentication;
using markit.Application.Contracts.Authentication.ExternalLogin;
using markit.Application.Contracts.GitHub;
using markit.Application.Contracts.Google;
using markit.Application.Exceptions;
using markit.Application.Features.Creators.Commands.CreateCreator;
using markit.Application.Models.Authentication;
using markit.Application.Models.Authentication.AppUser;
using markit.Application.Models.Authentication.Enums;
using markit.Application.Models.Settings;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static markit.Application.Helpers.GeneralConstant;

namespace markit.Infraestructure.Security.Services.ExternalLogin
{
    public class ExternalLoginService : IExternalLoginService
    {
        private readonly ILogger<ExternalLoginService> _logger;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IJwtService _jwtService;
        private readonly IGoogleApiService _googleApiService;
        private readonly IGitHubApiService _gitHubApiService;
        private readonly IExternalIdentifierService _externalIdentifierService;
        private readonly IExternalTokenService _externalTokenService;
        private readonly SpaSettings _spaSettings;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;

        private readonly LoginProvider[] _providers;
        private readonly string _spaRedirectUrl;

        public ExternalLoginService
        (
            ILogger<ExternalLoginService> logger,
            IMediator mediator,
            IMapper mapper,
            IJwtService jwtService,
            IOptions<SpaSettings> spaSettings,
            IGoogleApiService googleApiService,
            IGitHubApiService gitHubApiService,
            IExternalIdentifierService externalIdentifierService,
            IExternalTokenService externalTokenService,
            SignInManager<AppUser> signInManager,
            UserManager<AppUser> userManager)
        {
            _logger = logger;
            _mediator = mediator;
            _mapper = mapper;
            _jwtService = jwtService;
            _spaSettings = spaSettings.Value;
            _googleApiService = googleApiService;
            _gitHubApiService = gitHubApiService;
            _externalIdentifierService = externalIdentifierService;
            _externalTokenService = externalTokenService;
            _signInManager = signInManager;
            _userManager = userManager;

            _spaRedirectUrl = $"{_spaSettings.BaseUrl}/auth/redirect";
            _providers = [LoginProvider.Google, LoginProvider.GitHub];
        }

        #region Public
        public async Task<List<ExternalSignInMethod>> GetByUser(AppUser user)
        {
            List<ExternalSignInMethod> externalSignInMethods = [];
            IList<UserLoginInfo> logins = await _userManager.GetLoginsAsync(user);

            foreach (LoginProvider provider in _providers)
            {
                bool configured = logins.Any(l => l.LoginProvider.Equals(provider.GetName()));
                string? identifier = configured ? await _externalIdentifierService.GetAsync(provider, user) : default;
                externalSignInMethods.Add(
                    new ExternalSignInMethod(
                        LoginProvider: provider,
                        ProviderName: provider.GetName(),
                        identifier,
                        configured
                    )
                ); 
            }

            return externalSignInMethods;
        }

        public AuthenticationProperties GetAuthenticationProperties(
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

        public async Task<string> LoginCallback(LoginProvider provider, HttpContext context)
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
                await HandleApiTokenAccess(user, context);
                scope.Complete();
                // === Transaction end ===

                return $"{_spaRedirectUrl}?state=success&purpose=sign-in";
            }
            catch (Exception ex)
            {
                return LogAndRedirectError(ex.Message, _spaRedirectUrl);
            }
        }

        public async Task<string> LinkCallback(LoginProvider provider)
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
                    return $"{_spaRedirectUrl}?state=success&purpose=link";
                }

                throw new InvalidOperationException("Invalid or missed authentication properties.");
            }
            catch (Exception ex)
            {
                return LogAndRedirectError(ex.Message, _spaRedirectUrl);
            }
        }

        public async Task Remove(string userId, LoginProvider provider)
        {
            AppUser? user = await GetUser(userId);
            UserLoginInfo loginToRemove = await ValidateLoginRevoke(user, provider);

            bool accessRevoked = provider switch
            {
                LoginProvider.Google => await _googleApiService.RevokeAccessAsync(user),
                LoginProvider.GitHub => await _gitHubApiService.RevokeAccessAsync(user),
                _ => throw new InvalidOperationException($"Invalid or not implemented login provider: {provider.GetName()}")
            };

            if (!accessRevoked) throw new CustomValidationException("Revoke login failed: it wasn't possible to revoke provider access");

            try
            {
                using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);
                await _externalIdentifierService.Remove(provider, user);
                await _userManager.RemoveLoginAsync(user, provider.GetName(), loginToRemove.ProviderKey);
                scope.Complete();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to update account settings with userId: {{ID}}. The external access has been removed. {{reason}}", [userId, ex.Message]);
                throw;
            }
        }

        #endregion

        #region Helpers
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

        private async Task CreateCreator(AppUserRequest request)
        {
            CreateCreatorCommand command = _mapper.Map<CreateCreatorCommand>(request);
            await _mediator.Send(command);
        }

        private async Task HandleProviderTokens(
            IEnumerable<AuthenticationToken> providerTokens,
            LoginProvider provider,
            AppUser user
        )
        {
            await _externalTokenService.StoreAsync(provider, user, providerTokens);
        }

        private async Task HandleApiTokenAccess(AppUser user, HttpContext context)
        {
            TokenModel tokens = await _jwtService.GenerateTokens(user);
            _jwtService.SetInsideCookie(tokens, context);
        }

        private static AppUserRequest MapAppUserRequest(IEnumerable<Claim> claims, LoginProvider loginProvider)
        {
            ExternalAppUserGenerator generator = new(claims);
            return generator.Generate(loginProvider);
        }

        private string LogAndRedirectError(string message, string spaRedirectUrl)
        {
            _logger.LogError($"External login failed: {{Message}}", message);
            return $"{spaRedirectUrl}?state=failure&error={message}";
        }
        #endregion
    }
}
