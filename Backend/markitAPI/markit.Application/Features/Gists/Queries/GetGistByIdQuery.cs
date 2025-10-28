using markit.Application.Contracts.Authentication.ExternalLogin;
using markit.Application.Contracts.GitHub;
using markit.Application.Features.Gists.Queries.ViewModels;
using markit.Application.Models.Authentication;
using markit.Application.Models.Authentication.AppUser;
using markit.Application.Models.Authentication.Enums;
using MediatR;

namespace markit.Application.Features.Gists.Queries
{
    public class GetGistByIdQuery : IRequest<GistResponse>
    {
        public required string Id { get; set; }
        public required AppUser User { get; set; }
    }

    public class GetGistByIdQueryHandler : IRequestHandler<GetGistByIdQuery, GistResponse>
    {
        private readonly IGitHubApiService _gitHubApiService;
        private readonly IExternalLoginService _externalLoginService;

        public GetGistByIdQueryHandler(IGitHubApiService gitHubApiService, IExternalLoginService externalLoginService)
        {
            _gitHubApiService = gitHubApiService;
            _externalLoginService = externalLoginService;
        }

        public async Task<GistResponse> Handle(GetGistByIdQuery request, CancellationToken cancellationToken)
        {
            GistResponse response = new();
            if (!await HasGitHubLogin(request.User))
            {
                response.Status = GistResponseStatus.Failure;
                response.ErrorMessage = "It's necessary to connect your user with a GitHub account.";
                return response;
            }

            try
            {
                GistViewModel gist = await _gitHubApiService.GetGistById(request.Id, request.User);
                response.Status = GistResponseStatus.Success;
                response.Gist = gist;
                return response;
            }
            catch (InvalidOperationException ex)
            {
                response.Status = GistResponseStatus.Failure;
                response.ErrorMessage = ex.Message;
                return response;
            }
        }

        private async Task<bool> HasGitHubLogin(AppUser user)
        {
            ExternalSignInMethod? gitHubLogin = (await _externalLoginService.GetByUser(user))
                .FirstOrDefault(l => l.LoginProvider.Equals(LoginProvider.GitHub));
            return gitHubLogin?.Configured ?? false;
        }
    }
}
