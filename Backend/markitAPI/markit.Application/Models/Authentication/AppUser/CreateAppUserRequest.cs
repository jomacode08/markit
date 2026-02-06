using markit.Application.Models.Authentication.Enums;

namespace markit.Application.Models.Authentication.AppUser
{
    public class CreateAppUserRequest(
        string email,
        string name,
        int creatorId,
        AccessType accessType,
        string[] roles,
        string? password,
        string? picture
    )
    {
        private string _Email => email;
        public string Name => name;
        public int CreatorId => creatorId;
        public string? Password => password;
        public string? Picture => picture;
        public AccessType AccessType => accessType;
        public string[] Roles => roles;

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
