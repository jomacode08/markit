using markit.API.Controllers.Common;
using markit.Application.Features.Collections.Commands.CreateCollectionCommand;
using markit.Application.Features.Collections.Commands.DeleteCollectionCommand;
using markit.Application.Features.Collections.Commands.SetCollectionFavoriteStatusCommand;
using markit.Application.Features.Collections.Commands.UpdateCollectionCommand;
using markit.Application.Features.Collections.Queries.GetCollectionByIdQuery;
using markit.Application.Features.Collections.Queries.GetCollectionItemsPagedQuery;
using markit.Application.Features.Collections.Queries.GetMainCollectionByCreatorQuery;
using markit.Application.Features.Collections.Queries.ViewModels;
using markit.Infraestructure.Security.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace markit.API.Controllers.Operation
{
    [Authorize]
    public class CollectionsController : ApiControllerBase
    {
        private readonly IMediator _mediator;
        private readonly SessionService _sessionService;

        public CollectionsController(IMediator mediator, SessionService sessionService)
        {
            _mediator = mediator;
            _sessionService = sessionService;
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<CollectionViewModel>> GetCollectionById(int id)
        {
            GetCollectionByIdQuery collectionQuery = new(
                id,
                creatorId: _sessionService.GetCreatorId()
            );
            CollectionViewModel collection = await _mediator.Send(collectionQuery);
            return Ok(collection);
        }

        [HttpGet]
        [Route("main")]
        public async Task<ActionResult<CollectionViewModel>> GetMainCollectionByCurrentSession()
        {
            GetMainCollectionByCreatorQuery collectionQuery = new(creatorId : _sessionService.GetCreatorId());
            CollectionViewModel collection = await _mediator.Send(collectionQuery);
            return Ok(collection);
        }

        [HttpPost]
        [Route("search")]
        public async Task<ActionResult<CollectionItemPage>> GetItemsPaged([FromBody] GetCollectionItemsPagedQueryDto dto)
        {
            GetCollectionItemsPagedQuery query = new(
                dto,
                creatorId: _sessionService.GetCreatorId()
            );
            CollectionItemPage page = await _mediator.Send(query);
            return Ok(page);
        }

        [HttpPost]
        public async Task<ActionResult<CollectionViewModel>> Create([FromBody] CreateCollectionCommand command)
        {
            command.CreatorId = _sessionService.GetCreatorId();
            CollectionViewModel collection = await _mediator.Send(command);
            return Ok(collection);
        }

        [HttpPatch("{id:int}")]
        public async Task<ActionResult<CollectionViewModel>> Rename([FromRoute]int id, [FromBody] UpdateCollectionDto dto)
        {
            dto.CreatorId = _sessionService.GetCreatorId();
            UpdateCollectionCommand command = new(id, dto);
            CollectionViewModel updatedCollection = await _mediator.Send(command);
            return Ok(updatedCollection);
        }

        [HttpPut]
        [Route("{id:int}/favorite")]
        public async Task<IActionResult> SetFavoriteStatus(int id)
        {
            SetCollectionFavoriteStatusCommand command = new(
                id,
                creatorId: _sessionService.GetCreatorId(),
                isFavorite: true
            );
            await _mediator.Send(command);
            return Ok();
        }

        [HttpDelete]
        [Route("{id:int}/favorite")]
        public async Task<IActionResult> DeleteFavoriteStatus(int id)
        {
            SetCollectionFavoriteStatusCommand command = new(
                id,
                creatorId: _sessionService.GetCreatorId(),
                isFavorite: false
            );
            await _mediator.Send(command);
            return Ok();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> SoftDelete(int id)
        {
            SoftDeleteCollectionCommand command = new(
                id,
                creatorId: _sessionService.GetCreatorId()
            );
            await _mediator.Send(command);
            return Ok();
        }
    }
}
