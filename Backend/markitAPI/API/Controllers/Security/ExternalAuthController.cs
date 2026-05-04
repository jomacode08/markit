using markit.Application.Common.Helpers;
using markit.Application.Contracts.Authentication.ExternalLogin;
using markit.Application.Features.Settings.Commands.UpdateExternalAuthSettings;
using markit.Application.Features.Settings.Queries;
using markit.Application.Features.Settings.Queries.ViewModels;
using markit.Application.Models.Authentication.Enums;
using markit.Application.Models.Authentication.ExternalAuth;
using markit.Application.Models.Settings;
using markit.Infrastructure.Security.Services;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using static markit.Application.Helpers.GeneralConstant;

namespace markit.API.Controllers.Security
{
    [Authorize]
    [ApiController]
    [Route("auth/external")]
    public class ExternalAuthController : ControllerBase
    {
        private readonly IExternalLoginService _externalLoginService;
        private readonly IMemoryCache _cache;
        private readonly IMediator _mediator;
        private readonly SpaSettings _spaSettings;
        private readonly SessionService _sessionService;

        public ExternalAuthController(
            IExternalLoginService externalLoginService,
            IOptions<SpaSettings> spaSettings,
            IMemoryCache cache,
            IMediator mediator,
            SessionService sessionService)
        {
            _externalLoginService = externalLoginService;
            _spaSettings = spaSettings.Value;
            _sessionService = sessionService;
            _mediator = mediator;
            _cache = cache;
        }

        [Authorize(Policy = AuthorizationPolicies.CAN_ESCALATE)]
        [HttpGet("link-token")]
        public IActionResult GenerateLinkToken()
        {
            var userId = _sessionService.GetUserId();
            var token = Guid.NewGuid().ToString();
            _cache.Set(token, userId, TimeSpan.FromMinutes(2));
            return Ok(new { token });
        }

        [AllowAnonymous]
        [HttpGet("status")]
        public async Task<ActionResult<ExternalAuthStatus>> GetStatus()
        {
            bool isGoogleEnabled = await _externalLoginService.IsEnabledAsync(LoginProvider.Google);
            bool isGitHubEnabled = await _externalLoginService.IsEnabledAsync(LoginProvider.GitHub);
            return Ok(new ExternalAuthStatus(
                isGoogleEnabled,
                isGitHubEnabled
            ));
        }

        [AllowAnonymous]
        [HttpGet("{provider}")]
        public async Task<IActionResult> InitiateLogin(LoginProvider provider)
        {
            if (!await _externalLoginService.IsEnabledAsync(provider)) return NotFound();
            return InitiateChallenge(provider, purpose: LoginPurpose.SignIn);
        }

        [AllowAnonymous]
        [HttpGet("{provider}/account/{token}")]
        public async Task<IActionResult> InitiateLinkAccount(LoginProvider provider, string token)
        {
            if (!await _externalLoginService.IsEnabledAsync(provider)) return NotFound();
            string? userId = GetUserIdFromLinkToken(token);
            if (userId == null) return BadRequest("Invalid or expired link token.");

            return InitiateChallenge(
                provider, 
                purpose: LoginPurpose.LinkAccount,
                userId: userId
            );
        }

        [AllowAnonymous]
        [HttpGet("callback/{provider}")]
        public async Task<IActionResult> CallbackLogin(LoginProvider provider)
        {
            string redirectUrl = await _externalLoginService.LoginCallback(provider, HttpContext);
            return Redirect(redirectUrl);
        }

        [AllowAnonymous]
        [HttpGet("account/callback/{provider}")]
        public async Task<IActionResult> CallbackLinkAccount(LoginProvider provider)
        {
            string redirectUrl = await _externalLoginService.LinkCallback(provider);
            return Redirect(redirectUrl);
        }

        [Authorize(Policy = AuthorizationPolicies.CAN_ESCALATE)]
        [HttpDelete("{provider}")]
        public async Task<IActionResult> RemoveLogin(LoginProvider provider)
        {
            await _externalLoginService.Remove(_sessionService.GetUserId(), provider);
            return Ok();
        }


        [AllowAnonymous]
        [HttpGet("access-denied")]
        public IActionResult AccessDenied()
        {
            string spaLoginFailureUrl = $"{_spaSettings.BaseUrl}/auth/redirect?state=failure&error=access_denied";
            return Redirect(spaLoginFailureUrl);
        }

        [Authorize(Policy = AuthorizationPolicies.ADMIN_ONLY)]
        [HttpGet("settings")]
        public async Task<ActionResult<ExternalAuthSettings>> GetSettings()
        {
            return Ok(await _mediator.Send(new GetExternalAuthSettingsQuery()));
        }

        [Authorize(Policy = AuthorizationPolicies.ADMIN_ONLY)]
        [HttpPut("settings")]
        public async Task<ActionResult<ExternalAuthSettings>> UpdateSettings([FromBody] UpdateExternalAuthSettingsCommand command)
        {
            return Ok(await _mediator.Send(command));
        } 

        private ChallengeResult InitiateChallenge(LoginProvider provider, LoginPurpose purpose, string? userId = null)
        {
            // The url to redirect after the auth middleware successfully handle the external login operation.
            string? redirectUrl = GetRedirectUrl(provider, purpose);
            if (string.IsNullOrEmpty(redirectUrl)) throw new FormatException("The redirectUrl doesn't have the correct format");

            AuthenticationProperties properties = _externalLoginService.GetAuthenticationProperties(
                provider,
                purpose,
                redirectUrl,
                userId
            );

            return Challenge(properties, provider.GetName());
        }

        private string? GetRedirectUrl(LoginProvider provider, LoginPurpose purpose)
        {
            string? action = purpose switch
            {
                LoginPurpose.SignIn => nameof(CallbackLogin),
                LoginPurpose.LinkAccount => nameof(CallbackLinkAccount),
                _ => default,
            };

            if (action == null) return default;

            return Url.Action(
                action,
                controller: "ExternalAuth",
                values: new { provider = provider.GetName() },
                protocol: Request.Scheme
            );
        }

        private string? GetUserIdFromLinkToken(string token)
        {
            if (string.IsNullOrEmpty(token)) return default;
            if (_cache.TryGetValue(token, out string? value) && !string.IsNullOrEmpty(value))
            {
                _cache.Remove(token);
                return value;
            }

            return default;
        }
    }
}
