namespace markit.Application.Models.Authentication
{
    public record TokenModel(
        string AccessToken,
        string RefreshToken
    );
}
