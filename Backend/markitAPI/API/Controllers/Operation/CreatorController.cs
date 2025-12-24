using markit.API.Controllers.Common;
using markit.Application.Features.Creators.Commands.UpdateCreator;
using markit.Application.Features.Creators.Queries;
using markit.Application.Features.Creators.Queries.ViewModels;
using markit.Infraestructure.Security.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace markit.API.Controllers.Operation
{
    [Authorize]
    public class CreatorController : ApiControllerBase
    {
        private readonly IMediator _mediator;
        private readonly SessionService _sessionService;

        public CreatorController(IMediator mediator, SessionService sessionService)
        {
            _mediator = mediator;
            _sessionService = sessionService;
        }

        [HttpGet]
        [Route("me")]
        public async Task<ActionResult<CreatorViewModel>> GetByCurrentSession()
        {
            GetCreatorByIdQuery query = new(id: _sessionService.GetCreatorId());
            CreatorViewModel creator = await _mediator.Send(query);
            return Ok(creator);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<CreatorViewModel>> Update([FromRoute] int id, [FromBody] UpdateCreatorDto dto)
        {
            UpdateCreatorCommand command = new(id, dto);
            CreatorViewModel updatedCreator = await _mediator.Send(command);
            return Ok(updatedCreator);
        }
    }
}
