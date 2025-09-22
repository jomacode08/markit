using markit.Application.Contracts.Authentication;
using markit.Application.Models.Authentication;
using markit.Application.Models.Authentication.Enums;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Extensions;

namespace markit.API.Controllers.Security
{
    [Authorize]
    [Route("api/external-login")]
    public class ExternalLoginController : ControllerBase
    {
        private readonly IExternalLoginService _externalLoginService;
        private readonly SpaSettings _spaSettings;
        public ExternalLoginController(IExternalLoginService externalLoginService, IOptions<SpaSettings> spaSettings)
        {
            _externalLoginService = externalLoginService;
            _spaSettings = spaSettings.Value;
        }

        [AllowAnonymous]
        [HttpGet("initiate-google")]
        public IActionResult InitiateGoogle()
        {
            return Initiate(LoginProvider.Google);
        }

        [AllowAnonymous]
        [HttpGet("callback-google")]
        public async Task<IActionResult> CallbackGoogle()
        {
            string redirectUrl = await _externalLoginService.Callback(LoginProvider.Google);
            return Redirect(redirectUrl);
        }

        [AllowAnonymous]
        [HttpGet("access-denied")]
        public IActionResult AccessDenied()
        {
            string spaLoginFailureUrl = $"{_spaSettings.BaseUrl}/auth/redirect?error=access_denied";
            return Redirect(spaLoginFailureUrl);
        }

        private ChallengeResult Initiate(LoginProvider loginProvider)
        {
            // The url to redirect after the auth middleware successfully handle the external login from google
            string? redirectUrl = Url.Action(nameof(CallbackGoogle), "ExternalLogin", null, Request.Scheme);
            if (string.IsNullOrEmpty(redirectUrl)) throw new FormatException("The redirectUrl doesn't have the correct format");
            // Build authentication properties and return a 302 Redirect to google login's page. 
            AuthenticationProperties properties = _externalLoginService.GetExternalAuthenticationProperties(loginProvider, redirectUrl);
            return Challenge(properties, loginProvider.GetDisplayName());
        }
    }
}
