using markit.API.Controllers.Common;
using markit.Application.Features.Marks.Commands.CreateMarkCommand;
using markit.Application.Features.Marks.Commands.DeleteMarkCommand;
using markit.Application.Features.Marks.Commands.MoveMarkCommand;
using markit.Application.Features.Marks.Commands.RenameMarkCommand;
using markit.Application.Features.Marks.Commands.SetMarkFavoriteStatusCommand;
using markit.Application.Features.Marks.Commands.UpdateMarkCommand;
using markit.Application.Features.Marks.Queries;
using markit.Application.Features.Marks.Queries.ViewModels;
using markit.Application.Helpers;
using markit.Infraestructure.Security.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace markit.API.Controllers.Operation
{
    [Authorize]
    public class MarksController : ApiControllerBase
    {
        private readonly IMediator _mediator;
        private readonly SessionService _sessionService;

        public MarksController(IMediator mediator, SessionService sessionService)
        {
            _mediator = mediator;
            _sessionService = sessionService;
        }

        [HttpGet]
        [Route("{id:int}")]
        public async Task<ActionResult<MarkViewModel>> GetById(int id)
        {
            GetMarkByIdQuery query = new (
                id,
                creatorId: _sessionService.GetCreatorId()
            );

            return Ok(await _mediator.Send(query));
        }

        [HttpPost]
        public async Task<ActionResult<MarkViewModel>> Create([FromBody] CreateMarkCommand command)
        {
            command.CreatorId = _sessionService.GetCreatorId();
            MarkViewModel mark = await _mediator.Send(command);
            return Ok(mark);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<MarkViewModel>> Update([FromRoute]int id, [FromBody] UpdateMarkDto dto)
        {
            dto.CreatorId = _sessionService.GetCreatorId();
            UpdateMarkCommand command = new(id, dto);
            MarkViewModel updatedMark = await _mediator.Send(command);
            return Ok(updatedMark);
        }

        [HttpPatch("{id:int}")]
        public async Task<ActionResult<MarkViewModel>> Rename([FromRoute]int id, [FromBody] RenameMarkDto dto)
        {
            dto.CreatorId = _sessionService.GetCreatorId();
            RenameMarkCommand command = new(id, dto);
            MarkViewModel updatedMark = await _mediator.Send(command);
            return Ok(updatedMark);
        }

        [HttpPut]
        [Route("{id:int}/favorite")]
        public async Task<IActionResult> SetFavoriteStatus(int id)
        {
            SetMarkFavoriteStatusCommand command = new(
                id,
                creatorId: _sessionService.GetCreatorId(),
                isFavorite: true
            );
            await _mediator.Send(command);
            return NoContent();
        }

        [Authorize(GeneralConstant.AuthorizationPolicies.CAN_ESCALATE)]
        [HttpPost]
        [Route("{id:int}/move")]
        public async Task<IActionResult> Move([FromRoute] int id, [FromBody] MoveMarkCommandDto dto)
        {
            MoveMarkCommand command = new(
                markId: id,
                collectionId: dto.ParentId,
                creatorId: _sessionService.GetCreatorId()
            );

            await _mediator.Send(command);
            return NoContent();
        }

        [HttpDelete]
        [Route("{id:int}/favorite")]
        public async Task<IActionResult> DeleteFavoriteStatus(int id)
        {
            SetMarkFavoriteStatusCommand command = new(
                id,
                creatorId: _sessionService.GetCreatorId(),
                isFavorite: false
            );
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpDelete]
        [Route("{id:int}")]
        public async Task<IActionResult> SoftDelete(int id)
        {
            SoftDeleteMarkCommand command = new(
                id,
                creatorId: _sessionService.GetCreatorId()
            );
            await _mediator.Send(command);
            return NoContent();
        }
    }
}
