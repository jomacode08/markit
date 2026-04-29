using markit.Application.Common.Helpers;
using markit.Application.Contracts.Authentication;
using markit.Application.Contracts.Authentication.ExternalLogin;
using markit.Application.Contracts.GitHub;
using markit.Application.Contracts.Google;
using markit.Application.Exceptions;
using markit.Application.Models.Authentication;
using markit.Application.Models.Authentication.AppUser;
using markit.Application.Models.Authentication.Enums;
using markit.Application.Models.Settings;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Transactions;
using static markit.Application.Helpers.GeneralConstant;

namespace markit.Infrastructure.Security.Services.ExternalLogin
{
    public class ExternalLoginService : IExternalLoginService
    {
        private readonly ILogger<ExternalLoginService> _logger;
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

        private const string ACCOUNT_DOESNT_EXIST_ERROR_MESSAGE = "The account doesn't exist.";
        private const string AUTHENTICATION_TOKENS_NOT_FOUND_ERROR_MESSAGE = "The authentication tokens were not found.";
        private const string AUTHENTICATION_PROPERTIES_NOT_FOUND_ERROR_MESSAGE = "The authentication properties were not found.";
        private const string REVOKE_LOGIN_FAILED_ERROR_MESSAGE = "There was a problem while revoking the login, please try again later.";
        private const string LOGIN_INFO_NOT_FOUND_ERROR_MESSAGE = "The login information was not found, external cookie is lost or expired.";
        private const string NO_LINKED_ACCOUNT_TO_REMOVE_ERROR_MESSAGE = "There is no linked account to remove.";
        private const string PASSWORD_REQUIRED_TO_REMOVE_LINKED_ACCOUNT_ERROR_MESSAGE = "You need to set a password to delete your linked account.";
        private const string ACCOUNT_ALREADY_LINKED_ERROR_MESSAGE = "The account is already linked.";

        public ExternalLoginService
        (
            ILogger<ExternalLoginService> logger,
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
                // Extract external login information.
                ExternalLoginInfo loginInfo = await GetAndValidateLoginInfoAsync();
                if (loginInfo.AuthenticationTokens == null) throw new InvalidOperationException(AUTHENTICATION_TOKENS_NOT_FOUND_ERROR_MESSAGE);
                // Find user by external login.
                AppUser? user = await _userManager.FindByLoginAsync(loginInfo.LoginProvider, loginInfo.ProviderKey);
                bool requireAccountLinking = false;
                if (user is null)
                {
                    ExternalUser externalUser = ConstructExternalUserFromClaims(
                        claims: loginInfo.Principal.Claims,
                        loginProvider: provider
                    );
                    // Find user by the email included in claims.
                    user = await _userManager.FindByEmailAsync(externalUser.Email)
                    ?? throw new CustomValidationException(ACCOUNT_DOESNT_EXIST_ERROR_MESSAGE);
                    requireAccountLinking = true;
                }

                // === Transaction start ===
                using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);
                // Ensure external login existence.
                if (requireAccountLinking) await _userManager.AddLoginAsync(user, loginInfo);
                await HandleProviderTokens(
                    providerTokens: [.. loginInfo.AuthenticationTokens],
                    provider,
                    user
                );
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
                ExternalLoginInfo loginInfo = await GetAndValidateLoginInfoAsync();
                AuthenticationProperties properties = loginInfo.AuthenticationProperties
                    ?? throw new InvalidOperationException(AUTHENTICATION_PROPERTIES_NOT_FOUND_ERROR_MESSAGE);

                if (properties.Items.TryGetValue(AuthenticationItems.CURRENT_USERID_KEY, out string? userId) && userId != null)
                {
                    await ValidateExistentLogin(provider, loginInfo.ProviderKey);
                    AppUser user = await GetUser(userId);
                    using (TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        // Link user with the new login info.
                        await _userManager.AddLoginAsync(user, loginInfo);
                        // Get and store provider tokens.
                        if (loginInfo.AuthenticationTokens == null)
                            throw new InvalidOperationException(AUTHENTICATION_TOKENS_NOT_FOUND_ERROR_MESSAGE);
                        await HandleProviderTokens(
                            providerTokens: [.. loginInfo.AuthenticationTokens],
                            provider,
                            user
                        );
                        scope.Complete();
                    }
                    return $"{_spaRedirectUrl}?state=success&purpose=link";
                }

                throw new InvalidOperationException(AUTHENTICATION_PROPERTIES_NOT_FOUND_ERROR_MESSAGE);
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

            if (!accessRevoked) throw new CustomValidationException(REVOKE_LOGIN_FAILED_ERROR_MESSAGE);

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
        private async Task<ExternalLoginInfo> GetAndValidateLoginInfoAsync()
        {
            return await _signInManager.GetExternalLoginInfoAsync()
                ?? throw new InvalidOperationException(LOGIN_INFO_NOT_FOUND_ERROR_MESSAGE);
        }

        private async Task<AppUser> GetUser(string userId)
        {
            return await _userManager.FindByIdAsync(userId)
                ?? throw new NotFoundException("Users", userId);
        }

        private async Task<UserLoginInfo> ValidateLoginRevoke(AppUser user, LoginProvider provider)
        {
            IList<UserLoginInfo> logins = await _userManager.GetLoginsAsync(user);
            if (!logins.Any()) throw new InvalidOperationException(NO_LINKED_ACCOUNT_TO_REMOVE_ERROR_MESSAGE);

            bool hasPassword = await _userManager.HasPasswordAsync(user);
            if (!hasPassword && logins.Count <= 1) throw new CustomValidationException(PASSWORD_REQUIRED_TO_REMOVE_LINKED_ACCOUNT_ERROR_MESSAGE);

            return logins.FirstOrDefault(l => l.LoginProvider.Equals(provider.GetName()))
                ?? throw new InvalidOperationException($"There isn't a linked account for the {provider.GetName()} provider.");
        }

        private async Task ValidateExistentLogin(LoginProvider provider, string providerKey)
        {
            AppUser? userWithLogin = await _userManager.FindByLoginAsync(provider.GetName(), providerKey);
            if (userWithLogin != null) throw new CustomValidationException(ACCOUNT_ALREADY_LINKED_ERROR_MESSAGE);
        }

        private async Task HandleProviderTokens(
            List<AuthenticationToken> providerTokens,
            LoginProvider provider,
            AppUser user
        )
        {
            EnsureExpirationToken(providerTokens);
            await _externalTokenService.StoreAsync(provider, user, providerTokens);
        }

        private async Task HandleApiTokenAccess(AppUser user, HttpContext context)
        {
            TokenModel tokens = await _jwtService.GenerateTokenPairAsync(user);
            _jwtService.SetTokenPairInCookies(tokens, context);
        }

        private static ExternalUser ConstructExternalUserFromClaims(IEnumerable<Claim> claims, LoginProvider loginProvider)
        {
            ExternalUserGenerator generator = new(claims);
            return generator.Generate(loginProvider);
        }

        private string LogAndRedirectError(string message, string spaRedirectUrl)
        {
            _logger.LogError($"External login failed: {{Message}}", message);
            return $"{spaRedirectUrl}?state=failure&error={message}";
        }

        private static void EnsureExpirationToken(List<AuthenticationToken> tokens)
        {
            AuthenticationToken? expirationToken = tokens.FirstOrDefault(
                t => t.Name.Equals(Token.EXPIRES_AT_TOKEN_NAME, StringComparison.OrdinalIgnoreCase)
            );

            if (expirationToken is null)
            {
                tokens.Add(new AuthenticationToken()
                {
                    Name = Token.EXPIRES_AT_TOKEN_NAME,
                    Value = Token.LONG_LIVED_TOKEN_EXPIRES_AT_VALUE
                });
            }
        }
        #endregion
    }
}