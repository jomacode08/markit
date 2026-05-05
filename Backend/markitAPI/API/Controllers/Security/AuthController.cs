﻿﻿using markit.API.Controllers.Common;
using markit.Application.Contracts.Authentication;
using markit.Application.Contracts.Authentication.Demo;
using markit.Application.Contracts.Authentication.ExternalLogin;
using markit.Application.Exceptions;
using markit.Application.Helpers;
using markit.Application.Models.Authentication;
using markit.Application.Models.Authentication.AppUser;
using markit.Application.Models.Authentication.Enums;
using markit.Infrastructure.Security.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace markit.API.Controllers.Seguridad
{
    [Authorize]
    public class AuthController : ApiControllerBase
    {
        private readonly ILoginService _loginService;
        private readonly IExternalLoginService _externalLoginService;
        private readonly IDemoService _demoService;
        private readonly UserManager<AppUser> _userManager;
        private readonly SessionService _sessionService;

        public AuthController(
            ILoginService loginService,
            IExternalLoginService externalLoginService,
            IDemoService demoService,
            UserManager<AppUser> userManager,
            SessionService sessionService)
        {
            _loginService = loginService;
            _externalLoginService = externalLoginService;
            _demoService = demoService;
            _userManager = userManager;
            _sessionService = sessionService;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("login")]
        public async Task<ActionResult<AuthenticatedUser>> Authenticate(AuthRequest request)
        {
            AppUser user = await _loginService.LoginAsync(request, HttpContext);
            IReadOnlyList<string> roles = GetUserRoles();

            return Ok(new AuthenticatedUser(
                user.Id,
                user.GivenName,
                user.Email!,
                roles,
                user.Picture
            ));
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("options")]
        public async Task<ActionResult<AuthOptions>> GetAuthOptions()
        {
            bool isDemoAvailable = (await _demoService.GetStatusAsync()).Available;
            bool isGoogleAvailable = await _externalLoginService.IsEnabledAsync(LoginProvider.Google);
            bool isGitHubAvailable = await _externalLoginService.IsEnabledAsync(LoginProvider.GitHub);

            return Ok(new AuthOptions(
                isDemoAvailable,
                isGoogleAvailable,
                isGitHubAvailable
            ));
        }

        [HttpGet]
        [Route("me")]
        public ActionResult<AuthenticatedUser> IsAuthenticated()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            string? givenName = User.FindFirstValue(ClaimTypes.GivenName);
            string? email = User.FindFirstValue(ClaimTypes.Email);
            string? picture = User.FindFirstValue(GeneralConstant.CustomClaimType.ProfilePictureUrl);
            IReadOnlyList<string> roles = GetUserRoles();

            if (userId == null || givenName == null || email == null)
                throw new InvalidOperationException($"The user doesn't have the required claims.");

            return Ok(new AuthenticatedUser(
                userId,
                givenName,
                email,
                roles,
                picture
            ));
        }

        [HttpPost]
        [Route("logout")]
        public async Task<IActionResult> Logout()
        {
            string userId = _sessionService.GetUserId();
            AppUser user = await _userManager.FindByIdAsync(userId)
                ?? throw new NotFoundException("Users", userId);
            await _loginService.LogoutAsync(user, HttpContext);
            return Ok();
        }

        [HttpGet]
        [Route("signin-methods")]
        public async Task<ActionResult<SignInMethods>> GetSignInMethods()
        {
            string userId = _sessionService.GetUserId();
            AppUser user = await _userManager.FindByIdAsync(userId)
                ?? throw new NotFoundException("Users", userId);

            bool hasEmail = user.Email != null;
            bool hasPassword = await _userManager.HasPasswordAsync(user);
            List<ExternalSignInMethod> externalSignInMethods = await _externalLoginService.GetByUser(user);

            return Ok(new SignInMethods(
                hasEmail,
                hasPassword,
                externalSignInMethods
            ));
        }

        private IReadOnlyList<string> GetUserRoles()
        {
            return [.. User.Claims
                .Where(c => c.Type.Equals(ClaimTypes.Role))
                .Select(c => c.Value)
            ];
        }
    }
}
