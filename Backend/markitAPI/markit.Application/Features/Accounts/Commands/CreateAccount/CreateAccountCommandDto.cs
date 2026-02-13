using markit.Application.Models.Authentication.AppUser;

namespace markit.Application.Features.Accounts.Commands.CreateAccount
{
    public class AccountRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string[] Roles { get; set; } = [];
        public bool Enabled { get; set; } = false;

    }
    public class CreateAccountCommandDto
    {
        public required AccountRequest Account {  get; set; }
        public required PasswordRequest Password { get; set; }
    }
}
