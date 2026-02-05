using markit.Application.Models.Authentication.AppUser;

namespace markit.Application.Contracts.Authentication
{
    public interface IAppUserService
    {
        AppUser GetByCreatorId(int creatorId);
        Task<AppUserPaginationDto> GetPagedAsync(int page, int pageSize);
        Task CreateAsync(CreateAppUserRequest request);
        Task RenameAsync(RenameAppUserRequest request, int creatorId);
    }
}
