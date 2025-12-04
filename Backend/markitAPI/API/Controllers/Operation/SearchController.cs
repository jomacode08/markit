using markit.Application.Features.Reports.Search;
using markit.Application.Features.Reports.Search.ViewModels;
using markit.Application.Models.MeiliSearch.Search;
using markit.Infraestructure.Security.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace markit.API.Controllers.Operation
{
    [Authorize]
    [Route("api/search")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly SessionService _sessionService;

        public SearchController(IMediator mediator, SessionService sessionService)
        {
            _mediator = mediator;
            _sessionService = sessionService;
        }

        [HttpPost]
        [Route("documents")]
        public async Task<DocumentSearchVm> SearchDocuments([FromBody] DocumentSearchRequest request)
        {
            DocumentSearchQuery query = new()
            {
                Query = request.Query,
                Filter = request.Filter,
                CreatorId = _sessionService.GetCreatorId(),
            };
            return await _mediator.Send(query);
        }
    }
}
