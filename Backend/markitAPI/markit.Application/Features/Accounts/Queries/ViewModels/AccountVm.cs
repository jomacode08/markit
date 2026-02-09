namespace markit.Application.Features.Accounts.Queries.ViewModels
{
    public class AccountVm
    {
        public string UserId { get; init; } = string.Empty;
        public int CreatorId { get; init; }
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string UserName { get; init; } = string.Empty;
        public IReadOnlyList<string> Roles { get; init; } = [];
    }
}
