using markit.Application.Contracts.Authentication;
using Microsoft.AspNetCore.Http;
using static markit.Application.Helpers.GeneralConstant;

namespace markit.infrastructure.Security.Services
{
    public class SessionService : ISessionService
    {
        private const string EMAIL_CLAIM_TYPE = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress";
        private const string USER_ID_CLAIM_TYPE = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
        private readonly IHttpContextAccessor _contextAccessor;

        public SessionService(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        public string? GetIdentity()
        {
            if (_contextAccessor.HttpContext == null) return null;

            string? email = _contextAccessor.HttpContext.User.FindFirst(EMAIL_CLAIM_TYPE)?.Value;
            string? userId = _contextAccessor.HttpContext.User.FindFirst(USER_ID_CLAIM_TYPE)?.Value;

            if (email == null || userId == null) return null;

            return $"{email} - ID:{ userId }";
        }

        public string GetUserId()
        {
            return _contextAccessor.HttpContext?.User.FindFirst(USER_ID_CLAIM_TYPE)?.Value
                ?? throw new InvalidOperationException("The current session doesn't have the required nameidentifier claim.");
        }

        public int GetCreatorId()
        {
            string creatorIdClaim = _contextAccessor.HttpContext?.User.FindFirst(CustomClaimType.CreatorId)?.Value
                ?? throw new InvalidOperationException("The current session doesn't have the required creatorId claim.");

            if (!int.TryParse(creatorIdClaim, out int creatorId))
                throw new FormatException($"The creatorId claim doesn't have the right format ");
            
            return creatorId;
        }
    }
}
