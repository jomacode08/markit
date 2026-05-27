using markit.Application.Contracts.Authentication.Demo;
using markit.Application.Features.Settings.Commands.UpdateDemoSettings;
using markit.Application.Features.Settings.Queries;
using markit.Application.Features.Settings.Queries.ViewModels;
using markit.Application.Models.Authentication;
using markit.Application.Models.Authentication.AppUser;
using markit.Application.Models.Authentication.Demo;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using static markit.Application.Helpers.GeneralConstant;

namespace markit.API.Controllers.Security
{
    [Route("auth/[controller]")]
    [ApiController]
    public class DemoController : ControllerBase
    {
        private readonly IDemoService _demoService;
        private readonly IMediator _mediator;

        public DemoController(IDemoService demoService, IMediator mediator)
        {
            _demoService = demoService;
            _mediator = mediator;
        }

        [AllowAnonymous]
        [HttpPost]
        [EnableRateLimiting(RateLimiterPolicies.DEMO_LOGIN_QUOTA)]
        public async Task<ActionResult<AuthenticatedUser>> CreateSession([FromQuery][Required] string hostName)
        {
            AppUser demoUser = await _demoService.CreateSessionAsync(hostName, HttpContext);
            IReadOnlyList<string> roles = GetUserRolesFromClaims();
            if (demoUser.Email is null) return BadRequest("Demo user email is required.");

            return Ok(new AuthenticatedUser(
                demoUser.Id,
                demoUser.GivenName,
                demoUser.Email,
                roles,
                demoUser.Picture
            ));
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("status")]
        public async Task<ActionResult<DemoStatus>> GetStatus()
        {
            return Ok(await _demoService.GetStatusAsync());
        }

        [Authorize(Policy = AuthorizationPolicies.ADMIN_ONLY)]
        [HttpGet]
        [Route("settings")]
        public async Task<ActionResult<DemoSettings>> GetSettings()
        {
            return Ok(await _mediator.Send(new GetDemoSettingsQuery()));
        }

        [Authorize(Policy = AuthorizationPolicies.ADMIN_ONLY)]
        [HttpPut]
        [Route("settings")]
        public async Task<ActionResult<DemoSettings>> UpdateSettings([FromBody] UpdateDemoSettingsCommand command)
        {
            return Ok(await _mediator.Send(command));
        }

        private IReadOnlyList<string> GetUserRolesFromClaims()
        {
            return [.. User.Claims
                .Where(c => c.Type.Equals(ClaimTypes.Role))
                .Select(c => c.Value)
            ];
        }
    }
}
