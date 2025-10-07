using System.Security.Claims;
using markit.Application.Models.Authentication.AppUser;
using markit.Application.Models.Authentication.Enums;
using markit.Application.Common.Helpers;

namespace markit.Infraestructure.Security.Services.ExternalLogin
{
    public class ExternalAppUserGenerator(IEnumerable<Claim> claims)
    {
        public AppUserRequest Generate(LoginProvider provider)
        {
            return provider switch
            {
                LoginProvider.Google => GenerateForGoogle(),
                _ => throw new InvalidOperationException($"Invalid login provider: {provider.GetName()}")
            };
        }

        private AppUserRequest GenerateForGoogle()
        {
            string email = (claims.FirstOrDefault(c => c.Type.Equals("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"))?.Value)
            ?? throw new InvalidOperationException("The required 'emailaddress' claim was not provided by the external google login provider.");

            string givenName = (claims.FirstOrDefault(c => c.Type.Equals("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname"))?.Value)
            ?? throw new InvalidOperationException("The required 'givenname' claim was not provided by the external google login provider.");

            string surName = (claims.FirstOrDefault(c => c.Type.Equals("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname"))?.Value)
            ?? throw new InvalidOperationException("The required 'surname' claim was not provided by the external google login provider.");

            string? picture = claims.FirstOrDefault(c => c.Type.Equals("urn:google:picture"))?.Value;

            return new AppUserRequest(
                email,
                password: null,
                givenName,
                surName,
                picture,
                AccessType.External
            );
        }
    }
}
