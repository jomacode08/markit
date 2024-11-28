namespace markit.Application.Models.Authentication.AppUser
{
    public class UpdateAppUserRequest(
        string firstName,
        string lastName,
        bool registrationConfirmed
    )
    {
        public string FirstName => firstName;
        public string LastName => lastName;
        public bool RegistrationConfirmed => registrationConfirmed;
    }
}
