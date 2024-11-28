using markit.Application.Models.Authentication.AppUser;

namespace markit.Application.Contracts.Authentication
{
    public interface IAppUserService
    {
        Task CreateIdentityUser(CreateAppUserRequest request, int creatorId);
        Task UpdateIdentityUser(UpdateAppUserRequest request, int creatorId);
        AppUser GetAppUserByCreatorId(int creatorId);
    }
}
