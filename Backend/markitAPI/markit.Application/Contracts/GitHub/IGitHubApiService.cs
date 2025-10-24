using markit.Application.Contracts.Authentication.ExternalLogin.Common;
using markit.Application.Features.Gists.Queries.ViewModels;
using markit.Application.Models.Authentication.AppUser;
using markit.Application.Models.GitHub;

namespace markit.Application.Contracts.GitHub
{
    public interface IGitHubApiService : IExternalProviderBaseOperation
    {
        Task<GitHubProfileData> GetProfileAsync(AppUser user);
        Task<GistViewModel> GetGistById(string gistId, AppUser user);
    }
}
