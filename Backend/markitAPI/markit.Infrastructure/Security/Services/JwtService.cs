﻿using markit.Application.Common.Helpers;
using markit.Application.Contracts.Authentication;
using markit.Application.Contracts.Settings;
using markit.Application.Exceptions;
using markit.Application.Models.Authentication;
using markit.Application.Models.Authentication.AppUser;
using markit.Application.Models.Authentication.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using static markit.Application.Helpers.GeneralConstant;

namespace markit.Infrastructure.Security.Services
{
    public class JwtService : IJwtService
    {
        private readonly ISettingsService _settingsService;
        private readonly JwtSettings _jwtSettings;
        private readonly UserManager<AppUser> _userManager;

        public JwtService(
            ISettingsService settingsService,
            IOptions<JwtSettings> jwtSettings,
            UserManager<AppUser> userManager)
        {
            _settingsService = settingsService;
            _jwtSettings = jwtSettings.Value;
            _userManager = userManager;
        }

        public async Task<TokenModel> GenerateTokenPairAsync(AppUser user)
        {
            if (!user.Enabled) throw new CustomValidationException("The user does not have sufficient permissions to continue.");
            DateTime accessTokenExpirationDate = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenDurationInMinutes);
            string accessToken = await GenerateAccessTokenAsync(user, accessTokenExpirationDate);
            string refreshToken = GenerateRefreshToken();

            await StoreRefreshTokenInDatabase(
                user,
                refreshToken,
                DateTime.UtcNow.AddMinutes(_jwtSettings.RefreshTokenDurationInMinutes)
            );

            return new TokenModel(accessToken, refreshToken);
        }

        public void SetTokenPairInCookies(TokenModel tokenModel, HttpContext context)
        {
            DateTime expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.RefreshTokenDurationInMinutes);
            // Access token
            SetHttpOnlyCookie(
                context,
                key: Token.ACCESS_TOKEN_NAME,
                token: tokenModel.AccessToken,
                expiresAt
            );
            // RefreshToken
            SetHttpOnlyCookie(
                context,
                key: Token.REFRESH_TOKEN_NAME,
                token: tokenModel.RefreshToken,
                expiresAt
            );
        }

        public async Task<TokenModel> RefreshAsync(TokenModel tokens)
        {
            string accessToken = tokens.AccessToken;
            string refreshToken = tokens.RefreshToken;
            ClaimsPrincipal? principal;

            try
            {
                principal = ValidateExpiredToken(accessToken);
            }
            catch (Exception ex)
            {
                throw new UnauthorizedAccessException($"Invalid token access. { ex.Message }");
            }

            string userId = (principal.Claims.FirstOrDefault(c => c.Type.Equals(ClaimTypes.NameIdentifier))?.Value)
                ?? throw new InvalidOperationException($"The access token doesn't contain a required claim");

            AppUser user = await _userManager.FindByIdAsync(userId)
                ?? throw new NotFoundException("Users", userId);

            string? storedRefreshToken = await _userManager.GetAuthenticationTokenAsync(
                user,
                LoginProvider.Internal.GetName(),
                Token.REFRESH_TOKEN_NAME
            );

            string? refreshExpirationTimeStr = await _userManager.GetAuthenticationTokenAsync(
                user,
                LoginProvider.Internal.GetName(),
                Token.EXPIRES_AT_TOKEN_NAME
            );

            if (storedRefreshToken is null || refreshExpirationTimeStr is null)
                throw new UnauthorizedAccessException("The current user doesn't have a valid session");

            DateTime? expiresAt = ParseDateOrNull(refreshExpirationTimeStr);
            if (refreshToken != storedRefreshToken || expiresAt is null || expiresAt <= DateTime.UtcNow)
                throw new UnauthorizedAccessException("Invalid token refresh");
            return await GenerateTokenPairAsync(user);
        }

        public async Task RevokeAsync(AppUser user)
        {
            await _userManager.RemoveAuthenticationTokenAsync(
                user,
                LoginProvider.Internal.GetName(),
                Token.REFRESH_TOKEN_NAME
            );
            await _userManager.RemoveAuthenticationTokenAsync(
                user,
                LoginProvider.Internal.GetName(),
                Token.EXPIRES_AT_TOKEN_NAME
            );
        }

        public async Task IssueDemoTokenAsync(AppUser user, HttpContext context)
        {
            bool isDemo = await _userManager.IsInRoleAsync(user, Role.DEMO_NAME);
            if (!isDemo || !user.Enabled) throw new CustomValidationException("The user does not have sufficient permissions to continue.");
            string? demoTokenDurationInMinutesValue = await _settingsService.GetValueAsync(SystemConfigKeys.DEMO_TOKEN_DURATION_IN_MINUTES_KEY);

            if (!int.TryParse(demoTokenDurationInMinutesValue, out int demoTokenDurationInMinutes))
                throw new CustomValidationException("Demo mode is not available due to a configuration error.");

            DateTime expiresAt = DateTime.UtcNow.AddMinutes(demoTokenDurationInMinutes);
            string token = await GenerateAccessTokenAsync(user, expiresAt);
            SetHttpOnlyCookie(
                context,
                key : Token.ACCESS_TOKEN_NAME,
                token,
                expiresAt
            );
        }

        private ClaimsPrincipal ValidateExpiredToken(string accessToken)
        {
            TokenValidationParameters validationParameters = new()
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = false,
                ValidAudience = _jwtSettings.Audience,
                ValidIssuer = _jwtSettings.Issuer,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key)),
            };

            JwtSecurityTokenHandler tokenHandler = new();
            ClaimsPrincipal principal = tokenHandler.ValidateToken(
                accessToken,
                validationParameters,
                out SecurityToken securityToken
            );

            bool isInvalid = securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);
            if (isInvalid) throw new SecurityTokenException("Invalid token");

            return principal;
        }

        private async Task StoreRefreshTokenInDatabase(AppUser user, string refreshToken, DateTime expiresAt)
        {
            await _userManager.SetAuthenticationTokenAsync(
                user,
                loginProvider: LoginProvider.Internal.GetName(),
                tokenName: Token.REFRESH_TOKEN_NAME,
                tokenValue: refreshToken
            );
            await _userManager.SetAuthenticationTokenAsync(
                user,
                loginProvider: LoginProvider.Internal.GetName(),
                tokenName: Token.EXPIRES_AT_TOKEN_NAME,
                tokenValue: expiresAt.ToString("o")
            );
        }

        private void SetHttpOnlyCookie(HttpContext context, string key, string token, DateTime expiresAt)
        {
            CookieOptions cookieOptions = new()
            {
                Expires = expiresAt,
                HttpOnly = true,
                IsEssential = true,
                SameSite = SameSiteMode.None,
                Secure = true
            };

            context.Response.Cookies.Append(
                key,
                value: token,
                options: cookieOptions
            );
        }

        private async Task<string> GenerateAccessTokenAsync(AppUser user, DateTime expiresAt)
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

            JwtSecurityToken securityToken = new(
                    issuer: _jwtSettings.Issuer,
                    audience: _jwtSettings.Audience,
                    claims: claims,
                    expires: expiresAt,
                    signingCredentials: signingCredentials
            );

            return new JwtSecurityTokenHandler().WriteToken(securityToken);
        }

        private string GenerateRefreshToken()
        {
            byte[] randomNumber = new byte[32];
            using var randomNumberGenerator = RandomNumberGenerator.Create();
            randomNumberGenerator.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private async Task<IEnumerable<Claim>> GetUserRoleClaims(AppUser user)
        {
            IList<string> roles = await _userManager.GetRolesAsync(user);
            return roles.Select(r => new Claim(ClaimTypes.Role, r));
        }

        private static DateTime? ParseDateOrNull(string date)
        {
            if (DateTime.TryParse(date, out DateTime parsedDate))
            {
                return parsedDate;
            }
            return null;
        }
    }
}
