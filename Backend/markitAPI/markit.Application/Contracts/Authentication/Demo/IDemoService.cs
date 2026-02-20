using markit.Application.Models.Authentication.AppUser;
using Microsoft.AspNetCore.Http;

namespace markit.Application.Contracts.Authentication.Demo
{
    public interface IDemoService
    {
        Task<AppUser> LoginAsync(HttpContext context);
    }
}
