using markit.Application.Contracts.Authentication;
using markit.Application.Models.Authentication;
using markit.Application.Models.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace markit.API.Controllers.Seguridad
{
    [Authorize]
    [Route("api/login")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IAuthService _authService;

        public LoginController(IAuthService authService)
        {
            _authService = authService;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("authenticate")]
        public async Task<AuthResponse> Authenticate(AuthRequest request)
        {
            return await _authService.Login(request);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("authenticateByGoogle")]
        public async Task<AuthResponse> AuthenticateByGoogle(GoogleAuthRequest request)
        {
            return await _authService.LoginByGoogle(request);
        }
    }
}
