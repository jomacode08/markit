using markit.API.Controllers.Common;
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
    public class DocumentsController : ApiControllerBase
    {
        private readonly IMediator _mediator;
        private readonly SessionService _sessionService;

        public DocumentsController(IMediator mediator, SessionService sessionService)
        {
            _mediator = mediator;
            _sessionService = sessionService;
        }

        [HttpPost]
        [Route("search")]
        public async Task<ActionResult<DocumentSearchVm>> SearchDocuments([FromBody] DocumentSearchRequest request)
        {
            DocumentSearchQuery query = new()
            {
                Query = request.Query,
                Filter = request.Filter,
                CreatorId = _sessionService.GetCreatorId(),
            };
            DocumentSearchVm search = await _mediator.Send(query);
            return Ok(search);
        }
    }
}
