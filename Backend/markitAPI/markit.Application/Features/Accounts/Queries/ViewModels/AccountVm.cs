namespace markit.Application.Features.Accounts.Queries.ViewModels
{
    public class AccountVm
    {
        public string UserId { get; init; } = string.Empty;
        public string UserName { get; init; } = string.Empty;
        public string Name {  get; init; } = string.Empty;
        public bool Enabled { get; init; } = false;
        public IReadOnlyList<string> Roles { get; init; } = [];
    }
}
