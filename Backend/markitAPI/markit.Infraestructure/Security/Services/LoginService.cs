using markit.Application.Contracts.Authentication;
using markit.Application.Exceptions;
using markit.Application.Models.Authentication;
using markit.Application.Models.Authentication.AppUser;
using markit.Application.Models.Authentication.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Octokit;

namespace markit.Infraestructure.Security.Services
{
    public class LoginService : ILoginService
    {
        private readonly IJwtService _jwtService;
        private readonly JwtSettings _jwtSettings;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public LoginService(
            IJwtService jwtService,
            IOptions<JwtSettings> jwtSettings,
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _jwtSettings = jwtSettings.Value;
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

        private static void ValidateCreatorExistency(AppUser user)
        {
            if (user.CreatorId == null)
                throw new CustomValidationException("The user has not yet been fully configured");
        }

        private async Task HandleTokenAccess(AppUser user, HttpContext context)
        {
            string accessToken = await _jwtService.WriteToken(user);
            _jwtService.SetTokenInsideCookie(accessToken, context);
        }
    }
}
