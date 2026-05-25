using markit.Application.Contracts.Authentication;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Application.Models.Authentication.AppUser;
using markit.Application.Models.Authentication.Enums;
using markit.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Transactions;
using static markit.Application.Helpers.GeneralConstant;

namespace markit.Infrastructure.Security.Services
{
    public class AppUserService : IAppUserService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;

        public AppUserService(UserManager<AppUser> userManager, IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
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
                    UserName = u.UserName ?? "",
                    AccessType = u.AccessType,
                    CreatedDate = u.CreatedDate,
                    IsLocked = u.LockoutEnd != null && u.LockoutEnd > DateTime.UtcNow,
                    Enabled = u.Enabled
                }).ToListAsync();

            int totalItems = await _userManager.Users.CountAsync();
            return new AppUserPaginationDto(
                data,
                totalItems,
                page,
                pageSize
            );
        }

        public async Task<AppUser> CreateAsync(CreateAppUserRequest request)
        {
            AppUser? userInDatabase = await _userManager.FindByEmailAsync(request.Email);
            if (userInDatabase != null) throw new CustomValidationException($"The user with email: {request.Email} already exists.");
            if (!AreRolesValid(request.Roles)) throw new CustomValidationException("The user must have only permitted roles.");
            if (request.AccessType == AccessType.Internal && request.Password == null) 
                throw new CustomValidationException("The user must have a password.");

            using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);

            // User creation
            Collection mainCollection = ConstructMainCollection();
            AppUser identityUser = ConstructAppUser(request, mainCollection);
            IdentityResult registrationResult = identityUser.AccessType == AccessType.External
                ? await _userManager.CreateAsync(identityUser)
                : await _userManager.CreateAsync(identityUser, request.Password!);
            HandleIdentityResult(registrationResult);
            // Assign user to roles
            IdentityResult rolesResult = await _userManager.AddToRolesAsync(identityUser, request.Roles);
            HandleIdentityResult(rolesResult);
            // Update main collection path
            await UpdateMainCollectionPathAsync(mainCollection);

            scope.Complete();
            return identityUser;
        }

        public async Task<AppUser> UpdateAsync(UpdateAppUserRequest request)
        {
            AppUser user = await _userManager.FindByIdAsync(request.Id)
                ?? throw new NotFoundException("Users", request.Id);
            using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);
                user.GivenName = request.Name;
                user.Email = request.Email;
                user.UserName = request.Email;
                user.Enabled = request.Enabled;
                IdentityResult result = await _userManager.UpdateAsync(user);
                HandleIdentityResult(result);
                await UpdateRoles(request.Roles, user);
            scope.Complete();
            return user;
        }

        public async Task RenameAsync(RenameAppUserRequest request)
        {
            AppUser user = await _userManager.FindByIdAsync(request.Id)
                ?? throw new NotFoundException("Users", request.Id);
            user.GivenName = request.NewName;
            IdentityResult result = await _userManager.UpdateAsync(user);
            HandleIdentityResult(result);
        }

        #region Helpers
        private static bool AreRolesValid(string[] userRoles)
        {
            return userRoles.All(r => Role.All.Contains(r));
        }

        private static AppUser ConstructAppUser(CreateAppUserRequest request, Collection mainCollection)
        {
            return new AppUser()
            {
                GivenName = request.Name,
                Email = request.Email,
                UserName = request.Email,
                Picture = request.Picture,
                AccessType = request.AccessType,
                CreatedDate = DateTime.UtcNow,
                EmailConfirmed = request.AccessType == AccessType.External,
                Enabled = request.Enabled,
                Collections = [mainCollection]
            };
        }

        private static Collection ConstructMainCollection() => new()
        {
            Name = Marks.MAIN_COLLECTION_NAME,
            PathNames = $"/{Marks.MAIN_COLLECTION_NAME}",
            IsMain = true
        };

        private static void HandleIdentityResult(IdentityResult result)
        {
            if (!result.Succeeded)
            {
                string errorsMessage = $"Failed : {string.Join(",", result.Errors.Select(x => x.Description))}";
                throw new CustomValidationException(errorsMessage);
            }
        }

        private static bool HaveRolesChanged(string[] currentRoles, string[] newRoles)
        {
            currentRoles = [..currentRoles.OrderBy(r => r)];
            newRoles = [..newRoles.OrderBy(r => r)];
            return !currentRoles.SequenceEqual(newRoles);
        }

        private async Task UpdateRoles(string[] roles, AppUser user)
        {
            if (!AreRolesValid(roles)) throw new CustomValidationException("The user must have only permitted roles.");
            string[] currentRoles = [..await _userManager.GetRolesAsync(user)];
            if (!HaveRolesChanged(currentRoles, newRoles: roles)) return;
            List<string> rolesToRemove = [..currentRoles];
            List<string> rolesToAdd = [];

            foreach (string role in roles) {
                if (currentRoles.Contains(role)) {
                    rolesToRemove.Remove(role);
                }
                else
                {
                    rolesToAdd.Add(role);
                }
            }

            if (rolesToRemove.Count > 0)
            {
                IdentityResult result = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                HandleIdentityResult(result);
            }

            if (rolesToAdd.Count > 0)
            {
                IdentityResult result = await _userManager.AddToRolesAsync(user, rolesToAdd);
                HandleIdentityResult(result);
            }
        }

        private async Task UpdateMainCollectionPathAsync(Collection mainCollection)
        {
            mainCollection.Path = $"{mainCollection.Id}";
            await _unitOfWork.CollectionRepository.UpdateAsync(mainCollection);
        }
        #endregion
    }
}
