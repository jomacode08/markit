namespace markit.Application.Models.Authentication.AppUser
{
    public class UpdateAppUserRequest(
        string id,
        string name,
        string email,
        string[] roles
    )
    {
        public string Id => id;
        public string Name => name;
        public string Email => email;
        public string[] Roles => roles;
    }
}
