using markit.Application.Features.Collections.Commands.CreateCollectionCommand;
using markit.Application.Features.Collections.Commands.DeleteCollectionCommand;
using markit.Application.Features.Collections.Commands.UpdateCollectionCommand;
using markit.Application.Features.Collections.Queries.ViewModels;
using markit.Infraestructure.Security.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace markit.API.Controllers.Operation
{
    [Authorize]
    [Route("api/collections")]
    [ApiController]
    public class CollectionController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly SessionService _sessionService;

        public CollectionController(IMediator mediator, SessionService sessionService)
        {
            _mediator = mediator;
            _sessionService = sessionService;
        }

        [HttpPost]
        [Route("create")]
        public async Task<CollectionViewModel> Create([FromBody] CreateCollectionCommand command)
        {
            command.CreatorId = _sessionService.GetCreatorId();
            return await _mediator.Send(command);
        }

        [HttpPatch]
        [Route("update")]
        public async Task<CollectionViewModel> Update([FromBody] UpdateCollectionCommand command) => await _mediator.Send(command);

        [HttpDelete]
        [Route("softDelete/{CollectionId}")]
        public async Task<bool> SoftDelete([FromRoute] SoftDeleteCollectionCommand command) => await _mediator.Send(command);
    }
}
