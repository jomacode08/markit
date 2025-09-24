using markit.Application.Contracts.Authentication;
using markit.Application.Models.Authentication;
using markit.Application.Models.Authentication.AppUser;
using markit.Infraestructure.Security.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace markit.API.Controllers.Seguridad
{
    [Authorize]
    [Route("api/login")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly ILoginService _loginService;
        private readonly IExternalLoginService _externalLoginService;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly SessionService _sessionService;

        public LoginController(
            ILoginService loginService,
            IExternalLoginService externalLoginService,
            SessionService sessionService,
            SignInManager<AppUser> signInManager,
            UserManager<AppUser> userManager)
        {
            _loginService = loginService;
            _signInManager = signInManager;
            _sessionService = sessionService;
            _externalLoginService = externalLoginService;
            _userManager = userManager;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("authenticate")]
        public async Task<AuthResponse> Authenticate(AuthRequest request)
        {
            return await _loginService.Login(request);
        }

        [HttpPost]
        [Route("logout")]
        public async Task<IActionResult> Logout()
        {
            if (User == null || User.Identity == null) return Unauthorized();
            
            if (User.Identity.IsAuthenticated) {
                await _externalLoginService.RemoveExternalTokens(_sessionService.GetUserId());
                await _signInManager.SignOutAsync();
            }

            return Ok();
        }

        [HttpGet]
        [Route("signin-methods")]
        public async Task<SignInMethods> GetSignInMethods()
        {
            AppUser? user = await _userManager.GetUserAsync(User) 
                ?? throw new UnauthorizedAccessException();

            bool hasEmail = user.Email != null;
            bool hasPassword = await _userManager.HasPasswordAsync(user);
            List<ExternalSignInMethod> externalSignInMethods = await _externalLoginService.GetExternalSignInMethods(user);

            return new SignInMethods(
                hasEmail,
                hasPassword,
                externalSignInMethods
            );
        }
    }
}
