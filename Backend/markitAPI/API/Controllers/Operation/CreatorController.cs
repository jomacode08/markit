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
    [Route("api/creators")]
    [ApiController]
    public class CreatorController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly SessionService _sessionService;

        public CreatorController(IMediator mediator, SessionService sessionService)
        {
            _mediator = mediator;
            _sessionService = sessionService;
        }

        [HttpGet]
        [Route("getById/{Id}")]
        public async Task<CreatorViewModel> GetById([FromRoute] GetCreatorByIdQuery query)
            => await _mediator.Send(query);

        [HttpGet]
        [Route("getByCurrentSession")]
        public async Task<CreatorViewModel> GetByCurrentSession()
        {
            var query = new GetCreatorByIdQuery()
            { 
                Id = _sessionService.GetCreatorId() 
            };

            return await _mediator.Send(query);
        }

        [HttpPut]
        [Route("update")]
        public async Task<CreatorViewModel> Update([FromBody] UpdateCreatorCommand command)
        {
            return await _mediator.Send(command);
        }
    }
}
