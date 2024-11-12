using Google.Apis.Auth;
using markit.Application.Contracts.Authentication.Google;
using markit.Application.Exceptions;
using markit.Application.Models.Authentication;
using markit.Application.Models.Authentication.Enums;
using markit.Application.Models.Authentication.Google;
using Microsoft.Extensions.Options;
using static Google.Apis.Auth.GoogleJsonWebSignature;

namespace markit.Infraestructure.Security.Services.Google
{
    public class GoogleAuthenticationService : IGoogleAuthenticationService
    {
        private readonly GoogleAuthSettings _googleAuthSettings;

        public GoogleAuthenticationService(IOptions<GoogleAuthSettings> googleAuthSettings)
        {
            _googleAuthSettings = googleAuthSettings.Value;
        }

        /// <summary>
        /// Validates a Google Token Id
        /// </summary>
        /// <param name="token"></param>
        /// <returns>The email related to the google account</returns>
        public async Task<UserViewModel> ValidateGoogleTokenId(string token)
        {
            Payload payload;

            try
            {
                var validationSettings = new ValidationSettings() {
                    Audience = [_googleAuthSettings.ClientId],
                };

                payload = await ValidateAsync(token, validationSettings);
            }
            catch (InvalidJwtException ex)
            {
                // TODO: Register in log system
                throw new CustomValidationException("The token is invalid");
            }

            return new UserViewModel(
                payload.Email,
                null,
                payload.GivenName,
                payload.FamilyName,
                payload.Picture,
                AccessType.Google
            );
        }
    }
}
