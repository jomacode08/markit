using markit.Application.Contracts.Authentication.Demo;
using markit.Application.Models.Authentication;
using markit.Application.Models.Authentication.AppUser;
using markit.Application.Models.Authentication.Demo;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace markit.API.Controllers.Security
{
    [Route("auth/[controller]")]
    [ApiController]
    public class DemoController : ControllerBase
    {
        private readonly IDemoService _demoService;

        public DemoController(IDemoService demoService)
        {
            _demoService = demoService;
        }

        [HttpPost]
        public async Task<ActionResult<AuthenticatedUser>> Login()
        {
            AppUser demoUser = await _demoService.LoginAsync(HttpContext);
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

        [HttpGet]
        [Route("status")]
        public async Task<ActionResult<DemoStatus>> GetStatus()
        {
            return Ok(await _demoService.GetStatusAsync());
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
