using FluentValidation.Validators;
using markit.Application.Contracts.Authentication;
using markit.Application.Contracts.Authentication.ExternalLogin;
using markit.Application.Exceptions;
using markit.Application.Helpers;
using markit.Application.Models.Authentication;
using markit.Application.Models.Authentication.AppUser;
using markit.Infraestructure.Security.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace markit.API.Controllers.Seguridad
{
    [Authorize]
    [Route("api/login")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly ILoginService _loginService;
        private readonly IExternalLoginService _externalLoginService;
        private readonly UserManager<AppUser> _userManager;
        private readonly SessionService _sessionService;

        public LoginController(
            ILoginService loginService,
            IExternalLoginService externalLoginService,
            UserManager<AppUser> userManager,
            SessionService sessionService)
        {
            _loginService = loginService;
            _externalLoginService = externalLoginService;
            _userManager = userManager;
            _sessionService = sessionService;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("authenticate")]
        public async Task<AuthenticatedUser> Authenticate(AuthRequest request)
        {
            AppUser user = await _loginService.Login(request, HttpContext);
            return new AuthenticatedUser
            (
                user.Id,
                user.GivenName,
                user.Email!,
                user.Picture
            );
        }

        [HttpGet]
        [Route("isAuthenticated")]
        public AuthenticatedUser IsAuthenticated()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            string? givenName = User.FindFirstValue(ClaimTypes.GivenName);
            string? email = User.FindFirstValue(ClaimTypes.Email);
            string? picture = User.FindFirstValue(GeneralConstant.CustomClaimType.ProfilePictureUrl);

            if (userId == null || givenName == null || email == null)
                throw new InvalidOperationException($"The user doesn't have the required claims.");

            return new AuthenticatedUser
            (
                userId,
                givenName,
                email,
                picture
            );
        }

        [HttpPost]
        [Route("logout")]
        public async Task<IActionResult> Logout()
        {
            string userId = _sessionService.GetUserId();
            AppUser user = await _userManager.FindByIdAsync(userId)
                ?? throw new NotFoundException("Users", userId);
            await _loginService.Logout(user, HttpContext);
            return Ok();
        }

        [HttpGet]
        [Route("signin-methods")]
        public async Task<SignInMethods> GetSignInMethods()
        {
            string userId = _sessionService.GetUserId();
            AppUser user = await _userManager.FindByIdAsync(userId)
                ?? throw new NotFoundException("Users", userId);

            bool hasEmail = user.Email != null;
            bool hasPassword = await _userManager.HasPasswordAsync(user);
            List<ExternalSignInMethod> externalSignInMethods = await _externalLoginService.GetByUser(user);

            return new SignInMethods(
                hasEmail,
                hasPassword,
                externalSignInMethods
            );
        }
    }
}
