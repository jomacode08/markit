using markit.Application.Models.Authentication.AppUser;

namespace markit.Application.Contracts.Authentication
{
    public interface IAppUserService
    {
        AppUser GetByCreatorId(int creatorId);
        Task<AppUserPaginationDto> GetPagedAsync(int page, int pageSize);
        Task<AppUser> CreateAsync(CreateAppUserRequest request);
        Task<AppUser> UpdateAsync(UpdateAppUserRequest request);
        Task RenameAsync(RenameAppUserRequest request);
    }
}
