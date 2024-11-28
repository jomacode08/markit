using markit.Application.Contracts.Authentication;
using markit.Application.Exceptions;
using markit.Application.Models.Authentication.Enums;
using Microsoft.AspNetCore.Identity;
using static markit.Application.Helpers.GeneralConstant;
using markit.Application.Models.Authentication.AppUser;

namespace markit.Infraestructure.Security.Services
{
    public class AppUserService : IAppUserService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AppUserService(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task CreateIdentityUser(CreateAppUserRequest request, int creatorId)
        {
            // Validate the existency of the user
            AppUser? userInDatabase = await _userManager.FindByEmailAsync(request.Email);

            if (userInDatabase != null)
                throw new CustomValidationException($"The user with email: {request.Email} already exists.");

            if (request.AccessType == AccessType.Internal && request.Password == null)
                throw new CustomValidationException("The user must have a password");

            // IdentityUser registration
            AppUser identityUser = new()
            {
                CreatorId = creatorId,
                GivenName = $"{request.FirstName} {request.LastName}",
                Email = request.Email,
                UserName = request.Email,
                Picture = request.Picture,
                AccessType = request.AccessType,
                CreatedDate = DateTime.UtcNow,
                EmailConfirmed = request.AccessType == AccessType.Google
            };

            IdentityResult registrationResult = identityUser.AccessType == AccessType.Google
                ? await _userManager.CreateAsync(identityUser)
                : await _userManager.CreateAsync(identityUser, request.Password!);

            if (registrationResult.Succeeded)
            {
                // Role registration
                IdentityRole? role = await _roleManager.FindByNameAsync(Role.general);

                if (role != null)
                {
                    await _userManager.AddToRoleAsync(identityUser, role.Name!);
                }
            }
            else
            {
                throw new CustomValidationException($"{registrationResult.Errors.First().Description}");
            }
        }

        public async Task UpdateIdentityUser(UpdateAppUserRequest request, int creatorId)
        {
            AppUser appUser = GetAppUserByCreatorId(creatorId);
            appUser.GivenName = $"{request.FirstName} {request.LastName}";
            appUser.RegistrationConfirmed = request.RegistrationConfirmed;

            await _userManager.UpdateAsync(appUser);
        }


        public AppUser GetAppUserByCreatorId(int creatorId)
        {
            return _userManager.Users
                .Where(u => u.CreatorId.Equals(creatorId))
                .FirstOrDefault()
                ?? throw new NotFoundException("User with creatorId", creatorId);
        }
    }
}
