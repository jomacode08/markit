using markit.Application.Models.Authentication.Enums;

namespace markit.Application.Models.Authentication
{
    public record SignInMethods(bool HasEmail, bool HasPassword, List<ExternalSignInMethod> ExternalSignMethods);
    public record ExternalSignInMethod(LoginProvider LoginProvider, string? Identifier, bool Configured);
}
