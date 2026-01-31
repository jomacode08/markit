using FluentValidation;
using markit.Application.Contracts.Authentication;
using markit.Application.Exceptions;
using markit.Application.Models.Authentication.AppUser;
using markit.Application.Models.Authentication.Enums;
using Microsoft.AspNetCore.Identity;
using System.Transactions;
using static markit.Application.Helpers.GeneralConstant;

namespace markit.Infraestructure.Security.Services
{
    public class AppUserService : IAppUserService
    {
        private readonly UserManager<AppUser> _userManager;

        public AppUserService(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task CreateIdentityUser(AppUserRequest request)
        {
            AppUser? userInDatabase = await _userManager.FindByEmailAsync(request.Email);
            if (userInDatabase != null) throw new CustomValidationException($"The user with email: {request.Email} already exists.");
            if (!AreRolesValid(request.Roles)) throw new CustomValidationException("The user must have only permitted roles.");
            if (request.AccessType == AccessType.Internal && request.Password == null) 
                throw new CustomValidationException("The user must have a password.");

            using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);
                // User creation
                AppUser identityUser = ConstructAppUser(request);
                IdentityResult registrationResult = identityUser.AccessType == AccessType.External
                    ? await _userManager.CreateAsync(identityUser)
                    : await _userManager.CreateAsync(identityUser, request.Password!);
                if (!registrationResult.Succeeded) throw new CustomValidationException($"{registrationResult.Errors.First().Description}");
                // UserRoles creation
                await _userManager.AddToRolesAsync(identityUser, request.Roles);
            scope.Complete();
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

        #region Helpers
        private static bool AreRolesValid(string[] userRoles)
        {
            return userRoles.All(r => Role.All.Contains(r));
        }

        private static AppUser ConstructAppUser(AppUserRequest request)
        {
            return new AppUser()
            {
                CreatorId = request.CreatorId,
                GivenName = request.Name,
                Email = request.Email,
                UserName = request.Email,
                Picture = request.Picture,
                AccessType = request.AccessType,
                CreatedDate = DateTime.UtcNow,
                EmailConfirmed = request.AccessType == AccessType.External
            };
        }
        #endregion
    }
}
