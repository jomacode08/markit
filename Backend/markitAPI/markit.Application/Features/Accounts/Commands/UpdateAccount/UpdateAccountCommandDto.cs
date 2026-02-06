namespace markit.Application.Features.Accounts.Commands.UpdateAccount
{
    public class UpdateAccountCommandDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string[] Roles { get; set; } = [];
    }
}
