using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Application.Features.Accounts.Queries.ViewModels;
using markit.Application.Models.Authentication.AppUser;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace markit.Application.Features.Accounts.Queries.GetAccountByUserId
{
    public class GetAccountByUserIdQuery(string userId) : IRequest<AccountVm>
    {
        public string UserId { get; } = userId;
    }

    public class GetAccountByUserIdQueryHandler : IRequestHandler<GetAccountByUserIdQuery, AccountVm>
    {
        private readonly UserManager<AppUser> _userManager;

        public GetAccountByUserIdQueryHandler(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<AccountVm> Handle(GetAccountByUserIdQuery request, CancellationToken cancellationToken)
        {
            AppUser user = await GetUser(request.UserId);

            if (user.UserName is null)
                throw new CustomValidationException($"The user with id: { request.UserId } must have a userName configured.");

            return new AccountVm()
            {
                UserId = user.Id,
                Name = user.UserName,
                UserName = user.UserName,
                Enabled = user.Enabled,
                Roles = await GetUserRoles(user),
            };
        }

        private async Task<AppUser> GetUser(string userId)
        {
            return await _userManager.FindByIdAsync(userId)
                ?? throw new NotFoundException("Users", userId);
        }

        private async Task<string[]> GetUserRoles(AppUser user)
        {
            return [..await _userManager.GetRolesAsync(user)];
        }
    }
}