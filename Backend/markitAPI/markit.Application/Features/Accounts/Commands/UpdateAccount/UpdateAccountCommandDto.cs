namespace markit.Application.Features.Accounts.Commands.UpdateAccount
{
    public class UpdateAccountCommandDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string[] Roles { get; set; } = [];
    }
}
