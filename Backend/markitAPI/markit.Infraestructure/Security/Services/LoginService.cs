﻿﻿using markit.Application.Contracts.Authentication;
using markit.Application.Contracts.Authentication.ExternalLogin;
using markit.Application.Contracts.Settings;
using markit.Application.Exceptions;
using markit.Application.Models.Authentication;
using markit.Application.Models.Authentication.AppUser;
using markit.Application.Models.Authentication.Enums;
using markit.Application.Models.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Transactions;
using static markit.Application.Helpers.GeneralConstant;

namespace markit.Infraestructure.Security.Services
{
    public class LoginService : ILoginService
    {
        private readonly IJwtService _jwtService;
        private readonly IExternalTokenService _externalTokenService;
        private readonly ISettingsService _settingsService;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public LoginService(
            IJwtService jwtService,
            IExternalTokenService externalTokenService,
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            ISettingsService settingsService)
        {
            _jwtService = jwtService;
            _externalTokenService = externalTokenService;
            _userManager = userManager;
            _signInManager = signInManager;
            _settingsService = settingsService;
        }

        public async Task<AppUser> LoginAsync(AuthRequest request, HttpContext context)
        {
            AppUser? user = await _userManager.FindByEmailAsync(request.Email);

            // Validate the existency of the user
            if (user == null || !user.AccessType.Equals(AccessType.Internal))
                throw new CustomValidationException("Invalid email or password.");

            // Password validation
            SignInResult signInResult = await _signInManager
                .PasswordSignInAsync(
                    userName: user.UserName!, 
                    request.Password,
                    isPersistent: false, 
                    lockoutOnFailure: true
                );
            if (signInResult.IsLockedOut)
                throw new CustomValidationException("The account is locked out, try again later.");
            if (!signInResult.Succeeded)
                throw new CustomValidationException("Invalid email or password.");
            ValidateCreatorExistency(user);
            await HandleTokenAccessAsync(user, context);
            return user;
        }

        public async Task<AppUser> DemoAsync(HttpContext context)
        {
            string? isDemoEnabledValue = await _settingsService
                .GetValueAsync(SystemConfigKeys.IS_DEMO_ENABLED_KEY);
            // Validate Demo toggle.
            if (!bool.TryParse(isDemoEnabledValue, out bool isDemoEnabled) || !isDemoEnabled)
            {
                throw new CustomValidationException("Demo mode is currently disabled.");
            }
            // Get and validate configured demo user.
            string demoUserId = await _settingsService.GetValueAsync(SystemConfigKeys.DEMO_USER_ID_KEY)
                ?? throw new CustomValidationException("The demo user has not been configured in the application.");
            AppUser demoUser = await _userManager.FindByIdAsync(demoUserId)
                ?? throw new NotFoundException("Users", demoUserId);
            var roles = await _userManager.GetRolesAsync(demoUser);
            // Validate demo user to have the correct demo role.
            if (!roles.Contains(Role.DEMO_NAME) || roles.Count != 1 || !demoUser.Enabled)
            {
                await DisableDemoModeAsync();
                throw new CustomValidationException("Demo mode is not available due to a configuration error."); 
            }            
            // Authenticate demo user.
            await _signInManager.SignInAsync(demoUser, isPersistent: false);
            await HandleTokenAccessAsync(demoUser, context);
            return demoUser;
        }

        public async Task LogoutAsync(AppUser user, HttpContext context)
        {
            using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);
            await _externalTokenService.ClearShortLivedAsync(user.Id);
            await _jwtService.Revoke(user);
            context.Response.Cookies.Delete(Token.ACCESS_TOKEN_NAME);
            context.Response.Cookies.Delete(Token.REFRESH_TOKEN_NAME);
            scope.Complete();
        }

        private static void ValidateCreatorExistency(AppUser user)
        {
            if (user.CreatorId == null)
                throw new CustomValidationException("The user has not yet been fully configured");
        }

        private async Task HandleTokenAccessAsync(AppUser user, HttpContext context)
        {
            TokenModel tokens = await _jwtService.GenerateTokens(user);
            _jwtService.SetInsideCookie(tokens, context);
        }

        private async Task DisableDemoModeAsync()
        {
            string key = SystemConfigKeys.IS_DEMO_ENABLED_KEY;
            SystemConfig isDemoEnabledSetting = await _settingsService.GetAsync(key)
                ?? throw new NotFoundException("SystemConfigs", key);
            isDemoEnabledSetting.Value = "false";
            await _settingsService.UpdateAsync(isDemoEnabledSetting);
        }
    }
}
