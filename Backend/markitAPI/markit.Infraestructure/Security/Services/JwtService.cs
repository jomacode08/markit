using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using markit.Application.Models.Authentication;
using markit.Application.Contracts.Authentication;
using markit.Application.Models.Authentication.AppUser;
using markit.Application.Models.Authentication.MeiliSearch;
using Meilisearch;
using static markit.Application.Helpers.GeneralConstant;

namespace markit.Infraestructure.Security.Services
{
    public class JwtService : IJwtService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly MeiliSearchAuthSettings _meiliSearchAuthSettings;
        private readonly JwtSettings _jwtSettings;

        public JwtService(
            IOptions<MeiliSearchAuthSettings> meiliSearchAuthSettings,
            IOptions<JwtSettings> jwtSettings,
            UserManager<AppUser> userManager
        )
        {
            _meiliSearchAuthSettings = meiliSearchAuthSettings.Value;
            _jwtSettings = jwtSettings.Value;
            _userManager = userManager;
        }

        public async Task<AuthResponse> GenerateAuthResponse(AppUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            return new AuthResponse()
            {
                Token = GenerateToken(user, roles),
                MeiliSearchToken = await GenerateMeiliSearchTenantToken(user)
            };
        }

        #region Utilities
        private string GenerateToken(AppUser usuario, IList<string> roles)
        {
            var roleClaims = new List<Claim>();

            foreach (string rol in roles)
            {
                roleClaims.Add(new Claim(ClaimTypes.Role, rol));
            }

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.NameId, usuario.Id),
                new Claim(JwtRegisteredClaimNames.GivenName, usuario.GivenName),
                new Claim(JwtRegisteredClaimNames.Email, usuario.Email!),
                new Claim(CustomClaimType.ProfilePictureUrl, usuario.Picture ?? ""),
                new Claim(CustomClaimType.CreatorId, usuario.CreatorId.ToString()!)

            }
            .Union(roleClaims);

            var signingCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key)),
                SecurityAlgorithms.HmacSha256);

            var securityToken = new JwtSecurityToken(
                    issuer: _jwtSettings.Issuer,
                    audience: _jwtSettings.Audience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
                    signingCredentials: signingCredentials);

            return new JwtSecurityTokenHandler().WriteToken(securityToken);
        }

        private async Task<string> GenerateMeiliSearchTenantToken(AppUser user)
        {
            MeilisearchClient client = new(_meiliSearchAuthSettings.UrlServer, _meiliSearchAuthSettings.ApiKey);
            Key apiKeyInfo = await client.GetKeyAsync(_meiliSearchAuthSettings.ApiKey);
            var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes);
            var searchRules = new TenantTokenRules(new Dictionary<string, object> {
                { 
                    MeiliSearch.COLLECTION_INDEX_UID,
                    new Dictionary<string, object> { 
                        { "filter", $"creatorId = { user.CreatorId }" }
                    } 
                },
                {
                    MeiliSearch.MARK_INDEX_UID,
                    new Dictionary<string, object> {
                        { "filter", $"creatorId = { user.CreatorId }" }
                    }
                },
            });

            return client.GenerateTenantToken(
                apiKeyInfo.Uid,
                searchRules,
                apiKeyInfo.KeyUid,
                expiresAt
            );
        }
        #endregion
    }
}
