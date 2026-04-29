﻿﻿using markit.Application.Contracts.Authentication;
using markit.Application.Contracts.Authentication.ExternalLogin;
using markit.Application.Exceptions;
using markit.Application.Models.Authentication;
using markit.Application.Models.Authentication.AppUser;
using markit.Application.Models.Authentication.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Transactions;
using static markit.Application.Helpers.GeneralConstant;

namespace markit.Infrastructure.Security.Services
{
    public class LoginService : ILoginService
    {
        private readonly IJwtService _jwtService;
        private readonly IExternalTokenService _externalTokenService;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public LoginService(
            IJwtService jwtService,
            IExternalTokenService externalTokenService,
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager)
        {
            _jwtService = jwtService;
            _externalTokenService = externalTokenService;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<AppUser> LoginAsync(AuthRequest request, HttpContext context)
        {
            AppUser? user = await _userManager.FindByEmailAsync(request.Email);

            // Validate the existence of the user
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
            ValidateCreatorExistence(user);
            await IssueTokenPairAsync(user, context);
            return user;
        }

        public async Task LogoutAsync(AppUser user, HttpContext context)
        {
            bool isDemo = await IsDemoUser(user);
            using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);

            if (!isDemo) await _externalTokenService.ClearShortLivedAsync(user);
            await _jwtService.RevokeAsync(user);
            context.Response.Cookies.Delete(Token.ACCESS_TOKEN_NAME);
            context.Response.Cookies.Delete(Token.REFRESH_TOKEN_NAME);

            scope.Complete();
        }

        private static void ValidateCreatorExistence(AppUser user)
        {
            if (user.CreatorId == null)
                throw new CustomValidationException("The user has not yet been fully configured");
        }

        private async Task IssueTokenPairAsync(AppUser user, HttpContext context)
        {
            TokenModel tokens = await _jwtService.GenerateTokenPairAsync(user);
            _jwtService.SetTokenPairInCookies(tokens, context);
        }

        private async Task<bool> IsDemoUser(AppUser user)
        {
            return await _userManager.IsInRoleAsync(user, Role.DEMO_NAME);
        }
    }
}
