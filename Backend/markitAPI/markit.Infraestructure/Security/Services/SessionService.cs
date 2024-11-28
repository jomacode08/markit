using markit.Application.Contracts.Authentication;
using Microsoft.AspNetCore.Http;
using static markit.Application.Helpers.GeneralConstant;

namespace markit.Infraestructure.Security.Services
{
    public class SessionService : ISessionService
    {
        private readonly string EmailClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress";
        private readonly string UserIdClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
        private readonly IHttpContextAccessor _contextAccessor;

        public SessionService(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        public string? GetIdentity()
        {
            if (_contextAccessor.HttpContext == null) return null;

            string? email = _contextAccessor.HttpContext.User.FindFirst(EmailClaimType)?.Value;
            string? userId = _contextAccessor.HttpContext.User.FindFirst(UserIdClaimType)?.Value;

            if (email == null || userId == null) return null;

            return $"{email} - ID:{ userId }";
        }

        public int GetCreatorId()
        {
            string creatorIdStr = _contextAccessor.HttpContext?.User.FindFirst(CustomClaimType.CreatorId)?.Value
                ?? throw new ArgumentNullException(CustomClaimType.CreatorId);

            if (!int.TryParse(creatorIdStr, out int creatorId))
                throw new FormatException();
            
            return creatorId;
        }
    }
}
