namespace markit.Application.Models.Authentication.AppUser
{
    public class RenameAppUserRequest(
        string id,
        string newName,
        string newUserName
    )
    {
        public string Id => id;
        public string NewName => newName;
        public string NewUserName => newUserName;
    }
}
