namespace markit.Application.Features.Accounts.Commands.UpdateAccount
{
    public class UpdateAccountCommandDto
    {
        public string Name { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string[] Roles { get; set; } = [];
        public bool Enabled { get; set; } = false;
        public string? Password {  get; set; }
    }
}
