using markit.Application.Models.Authentication.AppUser;

namespace markit.Application.Contracts.Authentication
{
    public interface IAppUserService
    {
        Task CreateIdentityUser(AppUserRequest request);
        Task UpdateIdentityUser(UpdateAppUserRequest request, int creatorId);
        AppUser GetAppUserByCreatorId(int creatorId);
    }
}
