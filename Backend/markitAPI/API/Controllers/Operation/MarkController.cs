using markit.Application.Features.Marks.Commands.CreateMarkCommand;
using markit.Application.Features.Marks.Commands.DeleteMarkCommand;
using markit.Application.Features.Marks.Commands.RenameMarkCommand;
using markit.Application.Features.Marks.Commands.SetMarkFavoriteStatusCommand;
using markit.Application.Features.Marks.Commands.UpdateMarkCommand;
using markit.Application.Features.Marks.Queries;
using markit.Application.Features.Marks.Queries.ViewModels;
using markit.Infraestructure.Security.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace markit.API.Controllers.Operation
{
    [Authorize]
    [Route("api/marks")]
    [ApiController]
    public class MarkController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly SessionService _sessionService;

        public MarkController(IMediator mediator, SessionService sessionService)
        {
            _mediator = mediator;
            _sessionService = sessionService;
        }

        [HttpGet]
        [Route("getById/{Id}")]
        public async Task<MarkViewModel> GetById([FromRoute] GetMarkByIdQuery query)
            => await _mediator.Send(query);

        [HttpGet]
        [Route("getByCurrentSession")]
        public async Task<List<MarkViewModel>> GetByCurrentSession()
        {
            var query = new GetMarksByCreatorIdQuery()
            {
                CreatorId = _sessionService.GetCreatorId()
            };

            return await _mediator.Send(query);
        }

        [HttpPost]
        [Route("create")]
        public async Task<MarkViewModel> Create([FromBody] CreateMarkCommand command)
        {
            command.CreatorId = _sessionService.GetCreatorId();
            return await _mediator.Send(command);
        }

        [HttpPatch]
        [Route("update")]
        public async Task<MarkViewModel> Update([FromBody] UpdateMarkCommand command)
        {
            return await _mediator.Send(command);
        }

        [HttpPatch]
        [Route("rename")]
        public async Task<MarkViewModel> Rename([FromBody] RenameMarkCommand command)
        {
            command.CreatorId = _sessionService.GetCreatorId();
            return await _mediator.Send(command);
        }

        [HttpPatch]
        [Route("favorite")]
        public async Task<bool> SetFavoriteStatus([FromBody] SetMarkFavoriteStatusCommand command)
        {
            command.CreatorId = _sessionService.GetCreatorId();
            return await _mediator.Send(command);
        }

        [HttpDelete]
        [Route("softDelete/{Id}")]
        public async Task<bool> SoftDelete([FromRoute] SoftDeleteMarkCommand command)
        {
            return await _mediator.Send(command);
        }
    }
}
