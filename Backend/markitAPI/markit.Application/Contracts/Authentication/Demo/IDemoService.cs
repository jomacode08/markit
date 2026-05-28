using markit.Application.Features.Accounts.Queries.ViewModels;
using markit.Application.Models.Authentication.Demo;
using Microsoft.AspNetCore.Http;

namespace markit.Application.Contracts.Authentication.Demo
{
    public interface IDemoService
    {
        Task<AccountVm> CreateSessionAsync(string guestName, HttpContext context);
        Task<DemoStatus> GetStatusAsync();
    }
}
