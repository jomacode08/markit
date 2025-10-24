using markit.Application.Contracts.GitHub;
using markit.Application.Features.Gists.Queries.ViewModels;
using markit.Application.Models.Authentication.AppUser;
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

        public GetGistByIdQueryHandler(IGitHubApiService gitHubApiService)
        {
            _gitHubApiService = gitHubApiService;
        }

        public async Task<GistResponse> Handle(GetGistByIdQuery request, CancellationToken cancellationToken)
        {
            GistResponse response = new();
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
    }
}
