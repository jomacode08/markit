using markit.Application.Contracts.Authentication;
using markit.Application.Features.Accounts.Queries.ViewModels;
using markit.Application.Features.Creators.Commands.CreateCreator;
using markit.Application.Models.Authentication.AppUser;
using markit.Application.Models.Authentication.Enums;
using MediatR;
using System.Transactions;

namespace markit.Application.Features.Account.Commands.CreateAccount
{
    public class CreateAccountCommand : IRequest<AccountVm>
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public AccessType AccessType { get; set; }
        public string[] Roles { get; set; } = [];
        public string? Password { get; set; }
        public string? Picture { get; set; }
    }

    public class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, AccountVm>
    {
        private readonly IAppUserService _appUserService;
        private readonly IMediator _mediator;

        public CreateAccountCommandHandler(IAppUserService appUserService, IMediator mediator)
        {
            _appUserService = appUserService;
            _mediator = mediator;
        }

        public async Task<AccountVm> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
        {
            using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);
                int creatorId = await CreateCreator(request.FirstName, request.LastName);
                CreateAppUserRequest userRequest = ConstructAppUserRequest(request, creatorId);
                AppUser user = await _appUserService.CreateAsync(userRequest);
            scope.Complete();

            return new AccountVm()
            {
                UserId = user.Id,
                CreatorId = creatorId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                UserName = request.Email,
                Roles = request.Roles,
            };
        }

        private async Task<int> CreateCreator(string firstName, string lastName)
        {
            CreateCreatorCommand command = new()
            {
                FirstName = firstName,
                LastName = lastName,
            };
            return await _mediator.Send(command);
        }

        private static CreateAppUserRequest ConstructAppUserRequest(CreateAccountCommand request, int creatorId)
        {
            return new CreateAppUserRequest(
                email: request.Email,
                name: $"{request.FirstName} {request.LastName}",
                creatorId: creatorId,
                accessType: request.AccessType,
                roles: request.Roles,
                password: request.Password,
                picture: request.Picture
            );
        }
    }
}
