namespace markit.Application.Models.Authentication.AppUser
{
    public class UpdateAppUserRequest(
        string id,
        string name,
        string email,
        bool rolesChanged,
        string[] roles
    )
    {
        public string Id => id;
        public string Name => name;
        public string Email => email;
        public bool RolesChanged => rolesChanged;
        public string[] Roles => roles;
    }
}
