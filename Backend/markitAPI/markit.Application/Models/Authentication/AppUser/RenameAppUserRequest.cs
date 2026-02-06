namespace markit.Application.Models.Authentication.AppUser
{
    public class RenameAppUserRequest(
        string id,
        string newName
    )
    {
        public string Id => id;
        public string NewName => newName;
    }
}
