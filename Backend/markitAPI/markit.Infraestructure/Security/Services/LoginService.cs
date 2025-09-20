using markit.Application.Exceptions;
using markit.Application.Models.Authentication.AppUser;
using markit.Application.Models.Authentication.Enums;
using markit.Application.Models.Authentication;
using Microsoft.AspNetCore.Identity;
using markit.Application.Contracts.Authentication;
using Meilisearch;

namespace markit.Infraestructure.Security.Services
{
    public class LoginService : ILoginService
    {
        private readonly IJwtService _authService;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public LoginService(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IJwtService authService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _authService = authService;
        }

        public async Task<AuthResponse> Login(AuthRequest request)
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
            return await _authService.GenerateAuthResponse(user);
        }

        private static void ValidateCreatorExistency(AppUser user)
        {
            if (user.CreatorId == null)
                throw new CustomValidationException("The user has not yet been fully configured");
        }
    }
}
