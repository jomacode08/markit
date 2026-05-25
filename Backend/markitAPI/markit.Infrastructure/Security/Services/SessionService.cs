using markit.Application.Contracts.Authentication;
using Microsoft.AspNetCore.Http;

namespace markit.Infrastructure.Security.Services
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
    }
}
