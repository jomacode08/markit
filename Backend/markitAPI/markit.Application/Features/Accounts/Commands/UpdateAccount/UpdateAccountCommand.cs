using markit.Application.Contracts.Authentication;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Application.Models.Authentication.AppUser;
using markit.Domain.Entities;
using MediatR;
using System.Transactions;

namespace markit.Application.Features.Accounts.Commands.UpdateAccount
{
    public class UpdateAccountCommand(string userId, UpdateAccountCommandDto dto) : IRequest<Unit>
    {
        public string UserId { get; set; } = userId;
        public string FirstName { get; set; } = dto.FirstName;
        public string LastName { get; set; } = dto.LastName;
        public string Email { get; set; } = dto.Email;
        public string[] Roles { get; set; } = dto.Roles;
    }

    public class UpdateAccountCommandHandler : IRequestHandler<UpdateAccountCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAppUserService _appUserService;

        public UpdateAccountCommandHandler(IUnitOfWork unitOfWork, IAppUserService appUserService)
        {
            _unitOfWork = unitOfWork;
            _appUserService = appUserService;
        }

        public async Task<Unit> Handle(UpdateAccountCommand request, CancellationToken cancellationToken)
        {
            using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);
                AppUser user = await UpdateUser(new UpdateAppUserRequest(
                    id: request.UserId,
                    name: $"{ request.FirstName } { request.LastName }",
                    email: request.Email,
                    roles: request.Roles
                ));
                if (user.CreatorId is null) throw new NotFoundException("Creator for user", user.Id);
                await UpdateCreator(
                    firstName: request.FirstName,
                    lastName: request.LastName,
                    creatorId: (int)user.CreatorId
                );
            scope.Complete();
            return Unit.Value;
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
