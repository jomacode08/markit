using markit.Application.Models.Authentication.Enums;

namespace markit.Application.Models.Authentication.AppUser
{
    public class CreateAppUserRequest(
        string email,
        string? password,
        string firstName,
        string lastName,
        string? picture,
        AccessType accessType
    )
    {
        private string _Email => email;
        public string? Password => password;
        public string FirstName => firstName;
        public string LastName => lastName;
        public string? Picture => picture;
        public AccessType AccessType => accessType;

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
