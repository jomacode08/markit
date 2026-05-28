using markit.Application.Models.Authentication.Enums;

namespace markit.Application.Models.Authentication.AppUser
{
    public class CreateAppUserRequest(
        string email,
        string name,
        AccessType accessType,
        string[] roles,
        string? password,
        string? picture,
        bool enabled,
        DateTime? expiresAt
    )
    {
        private string _Email => email;
        public string Name => name;
        public string? Password => password;
        public string? Picture => picture;
        public AccessType AccessType => accessType;
        public string[] Roles => roles;
        public bool Enabled => enabled;
        public DateTime? ExpiresAt => expiresAt;

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
