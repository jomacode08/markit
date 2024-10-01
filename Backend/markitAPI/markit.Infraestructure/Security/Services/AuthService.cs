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
using markit.Application.Models.Authentication.Google;
using markit.Application.Models.Authentication;
using markit.Application.Models.Authentication.Enums;
using static markit.Application.Helpers.GeneralConstant;
using markit.Application.Contracts.Authentication.Google;

namespace markit.Infraestructure.Autentication
{
    public class AuthService : IAuthService
    {
        private readonly IGoogleAuthenticationService _googleAuthenticationService;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<User> _signInManager;
        private readonly JwtSettings _jwtSettings;

        public AuthService(
            IGoogleAuthenticationService googleAuthenticationService,
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<JwtSettings> jwtSettings)
        {
            _googleAuthenticationService = googleAuthenticationService;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<AuthResponse> Login(AuthRequest request)
        {
            // Validación de existencia de Usuario.
            User user = await _userManager.FindByEmailAsync(request.Email) ??
                throw new CustomValidationException($"The user with email: {request.Email} doesn't exist");

            // Validación de Password
            SignInResult signInResult = await _signInManager
                .PasswordSignInAsync(user.UserName!, request.Password, false, lockoutOnFailure: false);

            if (!signInResult.Succeeded) 
                throw new CustomValidationException("The password is incorrect.");

            return await GenerateAuthResponse(user);
        }

        public async Task<AuthResponse> LoginByGoogle(GoogleSignInRequest request)
        {
            // Validate Google TokenId
            UserOperationModel googleSignInResponse = await _googleAuthenticationService.ValidateGoogleTokenId(request.TokenId);

            // Get the internal user related to the email google account
            User user = await _userManager.FindByEmailAsync(googleSignInResponse.Email)
            // If the user doesn't exist yet, then it will be created
                     ?? await CreateUser(googleSignInResponse);

            return await GenerateAuthResponse(user);
        }

        private async Task<User> CreateUser(UserOperationModel request)
        {
            // Validate the existency of the user
            User? userInDatabase = await _userManager.FindByEmailAsync(request.Email);

            if (userInDatabase != null)
                throw new CustomValidationException($"The user with email: { request.Email } already exists.");

            if (request.AccessType == AccessType.Internal && request.Password == null)
                throw new CustomValidationException("The user must have a password");

            // User registration
            User user = new()
            {
                Email = request.Email,
                UserName = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Gender = request.Gender,
                BirthDate = request.BirthDate,
                Picture = request.Picture,
                AccessType = request.AccessType,
                CreatedDate = DateTime.UtcNow,
                EmailConfirmed = request.AccessType == AccessType.Google,
            };

            IdentityResult registrationResult = user.AccessType == AccessType.Google
                ? await _userManager.CreateAsync(user)
                : await _userManager.CreateAsync(user, request.Password!);

            // Role registration
            if (registrationResult.Succeeded)
            {
                IdentityRole? role = await _roleManager.FindByNameAsync(Role.general);

                if (role != null)
                {
                    await _userManager.AddToRoleAsync(user, role.Name!);
                }
            }
            else
            {
                throw new CustomValidationException($"{registrationResult.Errors.First().Description}");
            }

            return user;
        }

        private async Task<AuthResponse> GenerateAuthResponse(User user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            return new AuthResponse()
            {
                Token = GenerateToken(user, user.FullName, roles),
            };
        }

        private string GenerateToken(User usuario, string givenName, IList<string> roles)
        {
            var roleClaims = new List<Claim>();

            foreach (string rol in roles)
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
