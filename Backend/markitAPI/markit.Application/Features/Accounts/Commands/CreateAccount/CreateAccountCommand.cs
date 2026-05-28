using markit.Application.Contracts.Authentication;
using markit.Application.Features.Accounts.Queries.ViewModels;
using markit.Application.Models.Authentication.AppUser;
using markit.Application.Models.Authentication.Enums;
using MediatR;

namespace markit.Application.Features.Account.Commands.CreateAccount
{
    public class CreateAccountCommand : IRequest<AccountVm>
    {
        public string Name { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public AccessType AccessType { get; set; }
        public string[] Roles { get; set; } = [];
        public string? Password { get; set; }
        public string? Picture { get; set; }
        public bool Enabled { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }

    public class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, AccountVm>
    {
        private readonly IAppUserService _appUserService;

        public CreateAccountCommandHandler(IAppUserService appUserService)
        {
            _appUserService = appUserService;
        }

        public async Task<AccountVm> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
        {
            CreateAppUserRequest userRequest = ConstructAppUserRequest(request);
            AppUser user = await _appUserService.CreateAsync(userRequest);

            return new AccountVm()
            {
                UserId = user.Id,
                Name = user.GivenName,
                Enabled = user.Enabled,
                UserName = request.Name,
                Roles = request.Roles
            };
        }

        private static CreateAppUserRequest ConstructAppUserRequest(CreateAccountCommand request)
        {
            return new CreateAppUserRequest(
                email: request.UserName,
                name: request.Name,
                accessType: request.AccessType,
                roles: request.Roles,
                password: request.Password,
                picture: request.Picture,
                enabled: request.Enabled,
                expiresAt: request.ExpiresAt
            );
        }
    }
}
