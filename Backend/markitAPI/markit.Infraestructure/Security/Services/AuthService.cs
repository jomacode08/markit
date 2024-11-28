using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using markit.Application.Exceptions;
using markit.Application.Models.Authentication.Google;
using markit.Application.Models.Authentication;
using markit.Application.Models.Authentication.Enums;
using static markit.Application.Helpers.GeneralConstant;
using markit.Application.Contracts.Authentication.Google;
using AutoMapper;
using markit.Application.Contracts.Authentication;
using markit.Application.Features.Creators.Commands.CreateCreator;
using MediatR;
using markit.Application.Models.Authentication.AppUser;

namespace markit.Infraestructure.Security.Services
{
    public class AuthService : IAuthService
    {
        private readonly IGoogleAuthenticationService _googleAuthenticationService;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly JwtSettings _jwtSettings;

        public AuthService(
            IGoogleAuthenticationService googleAuthenticationService,
            IOptions<JwtSettings> jwtSettings,
            IMapper mapper,
            IMediator mediator,
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            RoleManager<IdentityRole> roleManager
        )
        {
            _googleAuthenticationService = googleAuthenticationService;
            _jwtSettings = jwtSettings.Value;
            _mapper = mapper;
            _mediator = mediator;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        #region Login Methods
        public async Task<AuthResponse> Login(AuthRequest request)
        {
            AppUser? user = await _userManager.FindByEmailAsync(request.Email);

            // Validate the existency of the user
            if (user == null || !user.AccessType.Equals(AccessType.Internal))
                throw new CustomValidationException($"The user with email: {request.Email} doesn't exist");

            // Password validation
            SignInResult signInResult = await _signInManager
                .PasswordSignInAsync(user.UserName!, request.Password, false, lockoutOnFailure: false);

            if (!signInResult.Succeeded) 
                throw new CustomValidationException("The password is incorrect.");

            ValidateCreatorExistency(user);
            return await GenerateAuthResponse(user);
        }

        public async Task<AuthResponse> LoginByGoogle(GoogleAuthRequest request)
        {
            // Validate Google TokenId
            GoogleAuthResponse googleUserData = await _googleAuthenticationService.ValidateGoogleTokenId(request.TokenId);

            // Get the internal user related to the email google account
            AppUser? appUser = await _userManager.FindByEmailAsync(googleUserData.Email);

            // If the user doesn't exist yet, then it will be created
            if (appUser == null)
            {
                await CreateCreator(new CreateAppUserRequest(
                    googleUserData.Email,
                    null,
                    googleUserData.FirstName,
                    googleUserData.LastName,
                    googleUserData.Picture,
                    AccessType.Google
                ));

                appUser = await _userManager.FindByEmailAsync(googleUserData.Email);
            }

            ValidateCreatorExistency(appUser!);
            return await GenerateAuthResponse(appUser!);
        }
        #endregion

        #region Utilities
        private async Task CreateCreator(CreateAppUserRequest request)
        {
            CreateCreatorCommand command = _mapper.Map<CreateCreatorCommand>(request);
            await _mediator.Send(command);
        }

        private async Task<AuthResponse> GenerateAuthResponse(AppUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            return new AuthResponse()
            {
                Token = GenerateToken(user, roles)
            };
        }

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

        private static void ValidateCreatorExistency(AppUser user)
        {
            if (user.CreatorId == null)
                throw new CustomValidationException("The user has not yet been fully configured");
        }
        #endregion
    }
}
