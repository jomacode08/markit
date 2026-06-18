using markit.API.Controllers.Common;
using markit.Application.Features.Notebooks.Commands.CreateNotebookCommand;
using markit.Application.Features.Notebooks.Commands.DeleteNotebookCommand;
using markit.Application.Features.Notebooks.Commands.MoveNotebookCommand;
using markit.Application.Features.Notebooks.Commands.RenameNotebookCommand;
using markit.Application.Features.Notebooks.Commands.SetNotebookFavoriteStatusCommand;
using markit.Application.Features.Notebooks.Commands.UpdateNotebookCommand;
using markit.Application.Features.Notebooks.Queries;
using markit.Application.Features.Notebooks.Queries.ViewModels;
using markit.Infrastructure.Security.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace markit.API.Controllers.Operation
{
    [Authorize]
    public class NotebooksController : ApiControllerBase
    {
        private readonly IMediator _mediator;
        private readonly SessionService _sessionService;

        public NotebooksController(IMediator mediator, SessionService sessionService)
        {
            _mediator = mediator;
            _sessionService = sessionService;
        }

        [HttpGet]
        [Route("{id:int}")]
        public async Task<ActionResult<NotebookViewModel>> GetById(int id)
        {
            GetNotebookByIdQuery query = new (
                id,
                userId: _sessionService.GetUserId()
            );

            return Ok(await _mediator.Send(query));
        }

        [HttpGet]
        [Route("search")]
        public async Task<ActionResult<IReadOnlyList<NotebookSearchResult>>> Search([FromQuery] string searchTerm)
        {
            SearchNotebooksQuery query = new(searchTerm, _sessionService.GetUserId());
            return Ok(await _mediator.Send(query));
        }

        [HttpPost]
        public async Task<ActionResult<NotebookViewModel>> Create([FromBody] CreateNotebookCommand command)
        {
            command.UserId = _sessionService.GetUserId();
            NotebookViewModel notebook = await _mediator.Send(command);
            return Ok(notebook);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<NotebookViewModel>> Update([FromRoute]int id, [FromBody] UpdateNotebookDto dto)
        {
            dto.UserId = _sessionService.GetUserId();
            UpdateNotebookCommand command = new(id, dto);
            NotebookViewModel updatedNotebook = await _mediator.Send(command);
            return Ok(updatedNotebook);
        }

        [HttpPatch("{id:int}")]
        public async Task<ActionResult<NotebookViewModel>> Rename([FromRoute]int id, [FromBody] RenameNotebookDto dto)
        {
            dto.UserId = _sessionService.GetUserId();
            RenameNotebookCommand command = new(id, dto);
            NotebookViewModel updatedNotebook = await _mediator.Send(command);
            return Ok(updatedNotebook);
        }

        [HttpPut]
        [Route("{id:int}/favorite")]
        public async Task<IActionResult> SetFavoriteStatus(int id)
        {
            SetNotebookFavoriteStatusCommand command = new(
                id,
                userId: _sessionService.GetUserId(),
                isFavorite: true
            );
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpPost]
        [Route("{id:int}/move")]
        public async Task<IActionResult> Move([FromRoute] int id, [FromBody] MoveNotebookCommandDto dto)
        {
            MoveNotebookCommand command = new(
                notebookId: id,
                collectionId: dto.ParentId,
                userId: _sessionService.GetUserId()
            );

            await _mediator.Send(command);
            return NoContent();
        }

        [HttpDelete]
        [Route("{id:int}/favorite")]
        public async Task<IActionResult> DeleteFavoriteStatus(int id)
        {
            SetNotebookFavoriteStatusCommand command = new(
                id,
                userId: _sessionService.GetUserId(),
                isFavorite: false
            );
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpDelete]
        [Route("{id:int}")]
        public async Task<IActionResult> SoftDelete(int id)
        {
            SoftDeleteNotebookCommand command = new(
                id,
                userId: _sessionService.GetUserId()
            );
            await _mediator.Send(command);
            return NoContent();
        }
    }
}
