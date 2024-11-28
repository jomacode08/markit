namespace markit.Application.Models.Authentication.Google
{
    public class GoogleAuthResponse(string email, string firstName, string lastName, string picture)
    {
        public string Email => email;
        public string FirstName => firstName;
        public string LastName => lastName;
        public string Picture => picture;
    }
}
