using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using markit.Application.Contracts.Autentication;
using markit.Infraestructure.Security.Models;
using markit.Application.Models.Autentication;
using markit.Application.Exceptions;

namespace markit.Infraestructure.Autentication
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<User> _signInManager;
        private readonly JwtSettings _jwtSettings;

        public AuthService(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<JwtSettings> jwtSettings
        )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<AuthResponse> Login(AuthRequest request)
        {
            // Validación de existencia de Usuario.
            User usuario = await _userManager.FindByEmailAsync(request.Email) ??
                throw new CustomValidationException($"The user with email: {request.Email} doesn't exist");

            // Validación de Password
            SignInResult signInResult = await _signInManager
                .PasswordSignInAsync(usuario.UserName!, request.Password, false, lockoutOnFailure: false);

            if (!signInResult.Succeeded) 
                throw new CustomValidationException("The password is incorrect.");

            // Consulta de roles
            var roles = await _userManager.GetRolesAsync(usuario);

            // Creación de respuesta exitosa
            AuthResponse authResponse = new()
            {
                Token = GenerateToken(usuario, usuario.FullName, roles),
            };

            return authResponse;
        }

        private string GenerateToken(User usuario, string givenName, IList<string> roles)
        {
            var roleClaims = new List<Claim>();

            foreach  (string rol in roles)
            {
                roleClaims.Add(new Claim(ClaimTypes.Role, rol));
            }

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.NameId, usuario.Id),
                new Claim(JwtRegisteredClaimNames.GivenName, givenName),
                new Claim(JwtRegisteredClaimNames.Email, usuario.Email!),
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
    }
}
