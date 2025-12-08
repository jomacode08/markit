using markit.Application.Contracts.Authentication;
using markit.Application.Helpers;
using markit.Application.Models.Authentication;
using markit.Application.Models.Authentication.AppUser;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static markit.Application.Helpers.GeneralConstant;

namespace markit.Infraestructure.Security.Services
{
    public class JwtService : IJwtService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly UserManager<AppUser> _userManager;

        public JwtService(
            IOptions<JwtSettings> jwtSettings,
            UserManager<AppUser> userManager
        )
        {
            _jwtSettings = jwtSettings.Value;
            _userManager = userManager;
        }

        public async Task<string> WriteToken(AppUser user)
        {
            IEnumerable<Claim> roleClaims = await GetUserRoleClaims(user);
            IEnumerable<Claim> claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.NameId, user.Id),
                new Claim(JwtRegisteredClaimNames.GivenName, user.GivenName),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim(CustomClaimType.ProfilePictureUrl, user.Picture ?? ""),
                new Claim(CustomClaimType.CreatorId, user.CreatorId.ToString()!)

            }
            .Union(roleClaims);

            SigningCredentials signingCredentials = new(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key)),
                SecurityAlgorithms.HmacSha256
            );

            JwtSecurityToken securityToken = new (
                    issuer: _jwtSettings.Issuer,
                    audience: _jwtSettings.Audience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
                    signingCredentials: signingCredentials
            );

            return new JwtSecurityTokenHandler().WriteToken(securityToken);
        }

        public void SetTokenInsideCookie(string accessToken, HttpContext context)
        {
            CookieOptions cookieOptions = new()
            {
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
                HttpOnly = true,
                IsEssential = true,
                SameSite = SameSiteMode.None,
                Secure = true,
                //SameSite = SameSiteMode.Lax,
                //Secure = false,
            };

            context.Response.Cookies.Append(
                key: Token.ACCESS_TOKEN_COOKIE_NAME,
                value: accessToken,
                cookieOptions
            );
        }

        private async Task<IEnumerable<Claim>> GetUserRoleClaims(AppUser user)
        {
            IList<string> roles = await _userManager.GetRolesAsync(user);
            return roles.Select(r => new Claim(ClaimTypes.Role, r));
        }
    }
}
