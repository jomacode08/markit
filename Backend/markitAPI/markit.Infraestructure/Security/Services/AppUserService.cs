using FluentValidation;
using markit.Application.Contracts.Authentication;
using markit.Application.Exceptions;
using markit.Application.Models.Authentication.AppUser;
using markit.Application.Models.Authentication.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

        public AppUser GetByCreatorId(int creatorId)
        {
            return _userManager.Users
                .Where(u => u.CreatorId.Equals(creatorId))
                .FirstOrDefault()
                ?? throw new NotFoundException("User with creatorId", creatorId);
        }

        public async Task<AppUserPaginationDto> GetPagedAsync(int page, int pageSize)
        {
            List<AppUserSummary> data = await _userManager.Users
                .AsNoTracking()
                .OrderByDescending(u => u.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new AppUserSummary
                {
                    Id = u.Id,
                    CreatorId = u.CreatorId,
                    UserName = u.UserName ?? "",
                    AccessType = u.AccessType,
                    CreatedDate = u.CreatedDate,
                    IsLocked = u.LockoutEnd > DateTime.UtcNow,
                    IsConfirmed = u.RegistrationConfirmed
                }).ToListAsync();

            int totalItems = await _userManager.Users.CountAsync();
            return new AppUserPaginationDto(
                data,
                totalItems,
                page,
                pageSize
            );
        }

        public async Task CreateAsync(CreateAppUserRequest request)
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

        public async Task RenameAsync(RenameAppUserRequest request, int creatorId)
        {
            AppUser appUser = GetByCreatorId(creatorId);
            appUser.GivenName = $"{request.FirstName} {request.LastName}";
            await _userManager.UpdateAsync(appUser);
        }

        #region Helpers
        private static bool AreRolesValid(string[] userRoles)
        {
            return userRoles.All(r => Role.All.Contains(r));
        }

        private static AppUser ConstructAppUser(CreateAppUserRequest request)
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
