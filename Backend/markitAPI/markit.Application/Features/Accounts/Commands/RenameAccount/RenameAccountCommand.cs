using markit.Application.Contracts.Authentication;
using markit.Application.Features.Accounts.Queries.ViewModels;
using markit.Application.Models.Authentication.AppUser;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace markit.Application.Features.Accounts.Commands.RenameAccount
{
    public class RenameAccountCommand(string userId, RenameAccountDto dto) : IRequest<AccountVm>
    {
        public string UserId { get; private set; } = userId;
        public string Name { get; private set; } = dto.Name;
        public string UserName { get; private set; } = dto.UserName;
    }

    public class RenameAccountCommandHandler : IRequestHandler<RenameAccountCommand, AccountVm>
    {
        private readonly IAppUserService _appUserService;
        private readonly UserManager<AppUser> _userManager;

        public RenameAccountCommandHandler(IAppUserService appUserService, UserManager<AppUser> userManager)
        {
            _appUserService = appUserService;
            _userManager = userManager;
        }

        public async Task<AccountVm> Handle(RenameAccountCommand request, CancellationToken cancellationToken)
        {
            AppUser user = await _appUserService.RenameAsync(new RenameAppUserRequest(
                id: request.UserId,
                newName: request.Name,
                newUserName: request.UserName
            ));

            IList<string> roles = await _userManager.GetRolesAsync(user);

            return new AccountVm
            {
                UserId = user.Id,
                UserName = user.UserName!,
                Name = user.GivenName,
                Enabled = user.Enabled,
                Picture = user.Picture,
                Roles = [..roles]
            };
        }
    }
}
