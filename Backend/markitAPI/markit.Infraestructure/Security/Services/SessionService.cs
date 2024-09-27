using markit.Application.Contracts.Authentication;
using Microsoft.AspNetCore.Http;

namespace markit.Infraestructure.Security.Services
{
    public class SessionService : ISessionService
    {
        private readonly string EmailClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress";
        private readonly string UserIdClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/sid";
        private readonly IHttpContextAccessor _contextAccessor;

        public SessionService(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        public string? GetSessionUserIdentification()
        {
            if (_contextAccessor.HttpContext == null) return null;

            string? email = _contextAccessor.HttpContext.User.FindFirst(EmailClaimType)?.Value;
            string? userId = _contextAccessor.HttpContext.User.FindFirst(UserIdClaimType)?.Value;

            if (email == null || userId == null) return null;

            return $"{email} - ID:{ userId }";
        }
    }
}
