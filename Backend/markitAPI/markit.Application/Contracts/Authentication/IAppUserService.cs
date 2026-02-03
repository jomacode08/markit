using markit.Application.Models.Authentication.AppUser;

namespace markit.Application.Contracts.Authentication
{
    public interface IAppUserService
    {
        AppUser GetUserByCreatorId(int creatorId);
        Task<AppUserPaginationDto> GetUsersPagedAsync(int page, int pageSize);
        Task CreateIdentityUserAsync(AppUserRequest request);
        Task UpdateIdentityUserAsync(UpdateAppUserRequest request, int creatorId);
    }
}
