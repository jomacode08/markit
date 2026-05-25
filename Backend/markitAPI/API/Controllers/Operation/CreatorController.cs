using markit.API.Controllers.Common;
using markit.Application.Features.Creators.Commands.UpdateCreator;
using markit.Application.Features.Creators.Queries;
using markit.Application.Features.Creators.Queries.ViewModels;
using markit.Application.Models.Authentication.AppUser;
using markit.Infrastructure.Security.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using static markit.Application.Helpers.GeneralConstant;

namespace markit.API.Controllers.Operation
{
    [Authorize]
    public class CreatorController : ApiControllerBase
    {
        private readonly IMediator _mediator;
        private readonly SessionService _sessionService;
        private readonly UserManager<AppUser> _userManager;

        public CreatorController(
            IMediator mediator,
            SessionService sessionService,
            UserManager<AppUser> userManager 
        )
        {
            _mediator = mediator;
            _sessionService = sessionService;
            _userManager = userManager;
        }

        [HttpGet]
        [Route("me")]
        public async Task<ActionResult<CreatorViewModel>> GetByCurrentSession()
        {
            throw new NotImplementedException();
            GetCreatorByIdQuery query = new(id: 0);
            CreatorViewModel creator = await _mediator.Send(query);
            return Ok(creator);
        }

        [Authorize(Policy = AuthorizationPolicies.CAN_ESCALATE)]
        [HttpPut("me")]
        public async Task<ActionResult<CreatorViewModel>> UpdateByCurrentSession([FromBody] UpdateCreatorDto dto)
        {
            throw new NotImplementedException();
            UpdateCreatorCommand command = new(
                id: 0,
                userId: _sessionService.GetUserId(),
                dto: dto
            );
            CreatorViewModel updatedCreator = await _mediator.Send(command);
            return Ok(updatedCreator);
        }
    }
}
