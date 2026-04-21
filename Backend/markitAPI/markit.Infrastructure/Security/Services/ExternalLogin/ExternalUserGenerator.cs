using System.Security.Claims;
using markit.Application.Models.Authentication.Enums;
using markit.Application.Common.Helpers;
using markit.Application.Models.Authentication;

namespace markit.infrastructure.Security.Services.ExternalLogin
{
    public class ExternalUserGenerator(IEnumerable<Claim> claims)
    {
        private const string IDENTITY_CLAIM_NAMESPACE = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims";
        private const string EMAIL_CLAIM_TYPE = $"{ IDENTITY_CLAIM_NAMESPACE }/emailaddress";
        private const string CLAIMS_NOT_FOUNDED_ERROR_MESSAGE = "Required claims missing or invalid";

        public ExternalUser Generate(LoginProvider provider)
        {
            return provider switch
            {
                LoginProvider.Google => GenerateForGoogle(),
                LoginProvider.GitHub => GenerateForGitHub(),
                _ => throw new InvalidOperationException($"Invalid login provider: {provider.GetName()}")
            };
        }

        private ExternalUser GenerateForGoogle()
        {
            const string GIVENNAME_CLAIM_TYPE = $"{ IDENTITY_CLAIM_NAMESPACE }/givenname";
            const string SURNAME_CLAIM_TYPE = $"{ IDENTITY_CLAIM_NAMESPACE }/surname";
            const string PICTURE_CLAIM_TYPE = "urn:google:picture";

            string? email = GetClaimValue(EMAIL_CLAIM_TYPE);
            string? givenName = GetClaimValue(GIVENNAME_CLAIM_TYPE);
            string? surName = GetClaimValue(SURNAME_CLAIM_TYPE);
            string? picture = GetClaimValue(PICTURE_CLAIM_TYPE);

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(givenName) || string.IsNullOrEmpty(surName)) {
                throw new InvalidOperationException(CLAIMS_NOT_FOUNDED_ERROR_MESSAGE);
            }

            return new ExternalUser(
                FirstName: givenName,
                LastName: surName,
                email,
                picture
            );
        }

        private ExternalUser GenerateForGitHub()
        {
            const string USER_NAME_CLAIM_TYPE = $"{IDENTITY_CLAIM_NAMESPACE}/name";
            const string NAME_CLAIM_TYPE = "urn:github:name";
            const string DEFAULT_FIRST_NAME = "Markit";
            const string DEFAULT_LAST_NAME = "Creator";

            string? email = GetClaimValue(EMAIL_CLAIM_TYPE);
            string? name = GetClaimValue(NAME_CLAIM_TYPE);
            string? userName = GetClaimValue(USER_NAME_CLAIM_TYPE);

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(userName)) {
                throw new InvalidOperationException(CLAIMS_NOT_FOUNDED_ERROR_MESSAGE);
            }

            return new ExternalUser(
                FirstName: name ?? DEFAULT_FIRST_NAME,
                LastName: DEFAULT_LAST_NAME,
                email
            );
        }

        private string? GetClaimValue(string claimType)
        {
            return claims.FirstOrDefault(c => c.Type.Equals(claimType))?.Value;
        }
    }
}
