using markit.Application.Features.Blocks.Commands.PatchBlockCommand;
using markit.Application.Features.Blocks.Queries.ViewModels;
using markit.Infraestructure.Security.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace markit.API.Controllers.Operation
{
    [Authorize]
    [Route("api/blocks")]
    [ApiController]
    public class BlockController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly SessionService _sessionService;
        public BlockController(IMediator mediator, SessionService sessionService)
        {
            _mediator = mediator;
            _sessionService = sessionService;
        }

        [HttpPatch]
        [Route("update-content")]
        public async Task<BlockViewModel> UpdateContent([FromBody] PatchBlockContentCommand command)
        {
            command.CreatorId = _sessionService.GetCreatorId();
            return await _mediator.Send(command);
        }
    }
}
