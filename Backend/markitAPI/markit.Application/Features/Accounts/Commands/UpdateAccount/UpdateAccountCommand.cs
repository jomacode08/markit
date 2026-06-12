using markit.Application.Contracts.Authentication;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Features.Accounts.Queries.ViewModels;
using markit.Application.Models.Authentication.AppUser;
using MediatR;

namespace markit.Application.Features.Accounts.Commands.UpdateAccount
{
    public class UpdateAccountCommand(string userId, UpdateAccountCommandDto dto) : IRequest<AccountVm>
    {
        public string UserId { get; set; } = userId;
        public string Name { get; set; } = dto.Name;
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
            AppUser user = await UpdateUser(new UpdateAppUserRequest(
                id: request.UserId,
                name: request.Name,
                email: request.UserName,
                roles: request.Roles,
                enabled : request.Enabled
            ));

            return new AccountVm()
            {
                UserId = user.Id,
                Name = request.Name,
                UserName = request.UserName,
                Roles = request.Roles,
                Enabled = user.Enabled,
            };
        }

        private async Task<AppUser> UpdateUser(UpdateAppUserRequest updateUserRequest)
        {
            return await _appUserService.UpdateAsync(updateUserRequest);
        }
    }
}
