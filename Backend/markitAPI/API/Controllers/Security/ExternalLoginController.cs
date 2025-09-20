using markit.Application.Contracts.Authentication;
using markit.Application.Models.Authentication.Enums;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Extensions;

namespace markit.API.Controllers.Security
{
    [Authorize]
    [Route("api/external-login")]
    public class ExternalLoginController : ControllerBase
    {
        private readonly IExternalLoginService _externalLoginService;
        public ExternalLoginController(IExternalLoginService externalLoginService)
        {
            _externalLoginService = externalLoginService;
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
