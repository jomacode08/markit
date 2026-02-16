using markit.Application.Contracts.Authentication;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Application.Features.Accounts.Queries.ViewModels;
using markit.Application.Models.Authentication.AppUser;
using markit.Domain.Entities;
using MediatR;
using System.Transactions;

namespace markit.Application.Features.Accounts.Commands.UpdateAccount
{
    public class UpdateAccountCommand(string userId, UpdateAccountCommandDto dto) : IRequest<AccountVm>
    {
        public string UserId { get; set; } = userId;
        public string FirstName { get; set; } = dto.FirstName;
        public string LastName { get; set; } = dto.LastName;
        public string UserName { get; set; } = dto.UserName;
        public string[] Roles { get; set; } = dto.Roles;
        public bool Enabled { get; set; } = dto.Enabled;

    }

    public class UpdateAccountCommandHandler : IRequestHandler<UpdateAccountCommand, AccountVm>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAppUserService _appUserService;

        public UpdateAccountCommandHandler(IUnitOfWork unitOfWork, IAppUserService appUserService)
        {
            _unitOfWork = unitOfWork;
            _appUserService = appUserService;
        }

        public async Task<AccountVm> Handle(UpdateAccountCommand request, CancellationToken cancellationToken)
        {
            using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);
                AppUser user = await UpdateUser(new UpdateAppUserRequest(
                    id: request.UserId,
                    name: $"{ request.FirstName } { request.LastName }",
                    email: request.UserName,
                    roles: request.Roles,
                    enabled : request.Enabled
                ));
                if (user.CreatorId is null) throw new NotFoundException("Creator for user", user.Id);
                await UpdateCreator(
                    firstName: request.FirstName,
                    lastName: request.LastName,
                    creatorId: (int)user.CreatorId
                );
            scope.Complete();

            return new AccountVm()
            {
                UserId = user.Id,
                CreatorId = user.CreatorId.Value,
                FirstName = request.FirstName,
                LastName = request.LastName,
                UserName = request.UserName,
                Roles = request.Roles,
                Enabled = user.Enabled,
            };
        }

        private async Task<AppUser> UpdateUser(UpdateAppUserRequest updateUserRequest)
        {
            return await _appUserService.UpdateAsync(updateUserRequest);
        }

        private async Task UpdateCreator(string firstName, string lastName, int creatorId) {
            Creator creator = await _unitOfWork.CreatorRepository.GetByIdAsync(creatorId)
                ?? throw new NotFoundException("Creators", creatorId);
            creator.FirstName = firstName;
            creator.LastName = lastName;
            await _unitOfWork.CreatorRepository.UpdateAsync(creator);
        }
    }
}
