using markit.Application.Models.Authentication.Enums;

namespace markit.Application.Models.Authentication.AppUser
{
    public class AppUserPaginationDto(
        IReadOnlyList<AppUserSummary> data,
        int totalItems,
        int page,
        int pageSize
    )
    {
        public IReadOnlyList<AppUserSummary> Data { get; } = data;
        public PaginationMetaData Meta { get; } = new(
            totalItems,
            TotalPages: totalItems == 0 ? 1 : (totalItems + pageSize - 1) / pageSize,
            CurrentPage: page,
            pageSize
        );
    }

    public record PaginationMetaData(
        int TotalItems,
        int TotalPages,
        int CurrentPage,
        int PageSize
    );

    public class AppUserSummary
    {
        public string Id { get; set; } = string.Empty;
        public int? CreatorId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public AccessType AccessType { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsLocked { get; set; }
        public bool IsConfirmed { get; set; }
    }
}
