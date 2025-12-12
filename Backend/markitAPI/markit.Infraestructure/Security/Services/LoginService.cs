using markit.Application.Contracts.Authentication;
using markit.Application.Contracts.Authentication.ExternalLogin;
using markit.Application.Exceptions;
using markit.Application.Helpers;
using markit.Application.Models.Authentication;
using markit.Application.Models.Authentication.AppUser;
using markit.Application.Models.Authentication.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Transactions;

namespace markit.Infraestructure.Security.Services
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

        public async Task<AppUser> Login(AuthRequest request, HttpContext context)
        {
            AppUser? user = await _userManager.FindByEmailAsync(request.Email);

            // Validate the existency of the user
            if (user == null || !user.AccessType.Equals(AccessType.Internal))
                throw new CustomValidationException($"The user with email: {request.Email} doesn't exist");

            // Password validation
            SignInResult signInResult = await _signInManager
                .PasswordSignInAsync(user.UserName!, request.Password, false, lockoutOnFailure: false);
            if (!signInResult.Succeeded)
                throw new CustomValidationException("The password is incorrect.");

            ValidateCreatorExistency(user);
            await HandleTokenAccess(user, context);
            return user;
        }

        public async Task Logout(AppUser user, HttpContext context)
        {
            using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);
            await _externalTokenService.ClearShortLivedAsync(user.Id);
            await _jwtService.Revoke(user);
            context.Response.Cookies.Delete(GeneralConstant.Token.ACCESS_TOKEN_NAME);
            context.Response.Cookies.Delete(GeneralConstant.Token.REFRESH_TOKEN_NAME);
            scope.Complete();
        }

        private static void ValidateCreatorExistency(AppUser user)
        {
            if (user.CreatorId == null)
                throw new CustomValidationException("The user has not yet been fully configured");
        }

        private async Task HandleTokenAccess(AppUser user, HttpContext context)
        {
            TokenModel tokens = await _jwtService.GenerateTokens(user);
            _jwtService.SetInsideCookie(tokens, context);
        }
    }
}
