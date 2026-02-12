namespace markit.Application.Models.Authentication.AppUser
{
    public class PasswordRequest
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
