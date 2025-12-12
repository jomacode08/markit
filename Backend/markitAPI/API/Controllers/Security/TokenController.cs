using markit.Application.Contracts.Authentication;
using markit.Application.Models.Authentication;
using Microsoft.AspNetCore.Mvc;
using static markit.Application.Helpers.GeneralConstant;

namespace markit.API.Controllers.Security
{
    [Route("api/token")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly IJwtService _jwtService;
        
        public TokenController(IJwtService jwtService)
        {
            _jwtService = jwtService;
        }

        [HttpPost]
        [Route("refresh")]
        public async Task<IActionResult> Refresh()
        {
            HttpContext.Request.Cookies.TryGetValue(Token.ACCESS_TOKEN_NAME, out string? accessToken);
            HttpContext.Request.Cookies.TryGetValue(Token.REFRESH_TOKEN_NAME, out string? refreshToken);

            if (accessToken is null || refreshToken is null) return BadRequest("Invalid token request");

            TokenModel renewedTokens = await _jwtService.Refresh(new TokenModel(accessToken, refreshToken));
            _jwtService.SetInsideCookie(renewedTokens, HttpContext);
            return Ok();
        }
    }
}
