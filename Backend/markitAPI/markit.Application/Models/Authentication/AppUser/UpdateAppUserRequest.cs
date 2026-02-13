namespace markit.Application.Models.Authentication.AppUser
{
    public class UpdateAppUserRequest(
        string id,
        string name,
        string email,
        string[] roles,
        bool enabled
    )
    {
        private string _Email => email;
        public string Id => id;
        public string Name => name;
        public string[] Roles => roles;
        public bool Enabled => enabled;
        public string Email
        {
            get
            {
                return _Email.ToLower();
            }

            private set { }
        }
    }
}
