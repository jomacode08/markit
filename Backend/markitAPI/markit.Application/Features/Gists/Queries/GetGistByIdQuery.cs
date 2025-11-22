using markit.Application.Contracts.Authentication.ExternalLogin;
using markit.Application.Contracts.GitHub;
using markit.Application.Features.Gists.Queries.ViewModels;
using markit.Application.Models.Authentication;
using markit.Application.Models.Authentication.AppUser;
using markit.Application.Models.Authentication.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace markit.Application.Features.Gists.Queries
{
    public class GetGistByIdQuery : IRequest<GistResponse>
    {
        public required string Id { get; set; }
        public required AppUser User { get; set; }
    }

    public class GetGistByIdQueryHandler : IRequestHandler<GetGistByIdQuery, GistResponse>
    {
        private ILogger<GetGistByIdQueryHandler> _logger;
        private readonly IGitHubApiService _gitHubApiService;
        private readonly IExternalLoginService _externalLoginService;

        public GetGistByIdQueryHandler(
            ILogger<GetGistByIdQueryHandler> logger,
            IGitHubApiService gitHubApiService,
            IExternalLoginService externalLoginService
        )
        {
            _logger = logger;
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
            catch (Exception ex)
            {
                _logger.LogError("GitHub gist retrieval process failed: {Message}", [ex.Message]);
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
