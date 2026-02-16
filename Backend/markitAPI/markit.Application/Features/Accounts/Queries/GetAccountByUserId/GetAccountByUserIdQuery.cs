using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Application.Features.Accounts.Queries.ViewModels;
using markit.Application.Models.Authentication.AppUser;
using markit.Domain.Entities;
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
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;

        public GetAccountByUserIdQueryHandler(IUnitOfWork unitOfWork, UserManager<AppUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<AccountVm> Handle(GetAccountByUserIdQuery request, CancellationToken cancellationToken)
        {
            AppUser user = await GetUser(request.UserId);

            if (user.CreatorId is null) 
                throw new CustomValidationException($"The user with id: { request.UserId } must have a creator configured.");
            if (user.UserName is null)
                throw new CustomValidationException($"The user with id: { request.UserId } must have a userName configured.");

            int creatorId = user.CreatorId.Value;
            Creator creator = await GetCreator(creatorId);

            return new AccountVm()
            {
                UserId = user.Id,
                CreatorId = creatorId,
                FirstName = creator.FirstName,
                LastName = creator.LastName,
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

        private async Task<Creator> GetCreator(int creatorId)
        {
            return await _unitOfWork.CreatorRepository.GetByIdAsync(creatorId)
                ?? throw new NotFoundException("Creators", creatorId);
        }
    }
}