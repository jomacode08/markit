using markit.Application.Features.Collections.Commands.CreateCollectionCommand;
using markit.Application.Features.Collections.Commands.DeleteCollectionCommand;
using markit.Application.Features.Collections.Commands.UpdateCollectionCommand;
using markit.Application.Features.Collections.Queries.GetCollectionItemsForGridQuery;
using markit.Application.Features.Collections.Queries.Grid;
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

        [HttpGet]
        [Route("getCollectionById/{CollectionId}")]
        public async Task<CollectionViewModel> GetCollectionById(int CollectionId)
        {
            var query = new GetCollectionByIdQuery
            {
                CollectionId = CollectionId,
                CreatorId = _sessionService.GetCreatorId(),
                IncludeCollectionITems = true
            };

            return await _mediator.Send(query);
        }

        [HttpGet]
        [Route("getRootCollectionsForGrid")]
        public async Task<List<CollectionItem>> GetRootItemsForGrid()
        {
            var query = new GetRootCollectionsForGridQuery
            {
                CreatorId = _sessionService.GetCreatorId(),
            };

            return await _mediator.Send(query);
        }

        [HttpPost]
        [Route("create")]
        public async Task<CollectionViewModel> Create([FromBody] CreateCollectionCommand command)
        {
            command.CreatorId = _sessionService.GetCreatorId();
            return await _mediator.Send(command);
        }

        [HttpPatch]
        [Route("rename")]
        public async Task<CollectionViewModel> Rename([FromBody] UpdateCollectionCommand command)
        {
            command.CreatorId = _sessionService.GetCreatorId();
           return await _mediator.Send(command);
        }

        [HttpDelete]
        [Route("softDelete/{CollectionId}")]
        public async Task<bool> SoftDelete(int CollectionId)
        {
            var command = new SoftDeleteCollectionCommand()
            {
                CollectionId = CollectionId,
                CreatorId = _sessionService.GetCreatorId()
            };

            return await _mediator.Send(command);
        }
    }
}
