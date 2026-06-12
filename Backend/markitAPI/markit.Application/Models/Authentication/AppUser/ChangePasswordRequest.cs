namespace markit.Application.Models.Authentication.AppUser
{
    public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
}
