using markit.API.Controllers.Common;
using markit.Application.Features.Blocks.Commands.PatchBlockCommand;
using markit.Application.Features.Blocks.Commands.PatchBlockContentCommand;
using markit.Application.Features.Blocks.Queries.ViewModels;
using markit.Infraestructure.Security.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace markit.API.Controllers.Operation
{
    [Authorize]
    public class BlocksController : ApiControllerBase
    {
        private readonly IMediator _mediator;
        private readonly SessionService _sessionService;
        public BlocksController(IMediator mediator, SessionService sessionService)
        {
            _mediator = mediator;
            _sessionService = sessionService;
        }

        [HttpPatch("{id:int}")]
        public async Task<ActionResult<BlockViewModel>> UpdateContent(int id, [FromBody] PatchBlockDto dto)
        {
            dto.CreatorId = _sessionService.GetCreatorId();
            PatchBlockContentCommand command = new(id, dto);
            BlockViewModel updatedBlock = await _mediator.Send(command);
            return Ok(updatedBlock);
        }
    }
}
