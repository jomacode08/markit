namespace markit.Application.Models.Authentication.AppUser
{
    public class RenameAppUserRequest(
        string firstName,
        string lastName
    )
    {
        public string FirstName => firstName;
        public string LastName => lastName;
    }
}
