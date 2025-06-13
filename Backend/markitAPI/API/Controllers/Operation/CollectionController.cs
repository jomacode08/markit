using markit.Application.Features.Collections.Commands.CreateCollectionCommand;
using markit.Application.Features.Collections.Commands.DeleteCollectionCommand;
using markit.Application.Features.Collections.Commands.SetCollectionFavoriteStatusCommand;
using markit.Application.Features.Collections.Commands.UpdateCollectionCommand;
using markit.Application.Features.Collections.Queries.GetCollectionByIdQuery;
using markit.Application.Features.Collections.Queries.GetCollectionChildrenPagedQuery;
using markit.Application.Features.Collections.Queries.GetMainCollectionByCreatorQuery;
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
        [Route("getById/{CollectionId}")]
        public async Task<CollectionViewModel> GetCollectionById(int collectionId)
        {
            var collectionQuery = new GetCollectionByIdQuery
            {
                CollectionId = collectionId,
                CreatorId = _sessionService.GetCreatorId()
            };

            return await _mediator.Send(collectionQuery);
        }

        [HttpGet]
        [Route("getMainByCurrentSession")]
        public async Task<CollectionViewModel> GetMainCollectionByCurrentSession()
        {
            var collectionQuery = new GetMainCollectionByCreatorQuery
            {
                CreatorId = _sessionService.GetCreatorId()
            };

            return await _mediator.Send(collectionQuery);
        }

        [HttpPost]
        [Route("getChildrenPaged")]
        public async Task<CollectionItemPage> GetCollectionChildrenPaged([FromBody] CollectionItemPageRequest request)
        {
            var query = new GetCollectionChildrenPagedQuery
            {
                CreatorId = _sessionService.GetCreatorId(),
                CollectionId = request.CollectionId,
                PageSize = request.PageSize,
                Cursor = request.Cursor,
                Filter = request.Filter
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

        [HttpPatch]
        [Route("favorite")]
        public async Task<bool> SetFavoriteStatus([FromBody] SetCollectionFavoriteStatusCommand command)
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
