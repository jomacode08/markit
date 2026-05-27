using markit.Application.Models.Authentication.AppUser;
using markit.Application.Models.Authentication.Demo;
using Microsoft.AspNetCore.Http;

namespace markit.Application.Contracts.Authentication.Demo
{
    public interface IDemoService
    {
        Task<AppUser> CreateSessionAsync(string guestName, HttpContext context);
        Task<DemoStatus> GetStatusAsync();
    }
}
