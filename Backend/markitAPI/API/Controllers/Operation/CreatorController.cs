using markit.API.Controllers.Common;
using markit.Application.Exceptions;
using markit.Application.Features.Creators.Commands.UpdateCreator;
using markit.Application.Features.Creators.Queries;
using markit.Application.Features.Creators.Queries.ViewModels;
using markit.Application.Models.Authentication.AppUser;
using markit.infrastructure.Security.Services;
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
            GetCreatorByIdQuery query = new(id: _sessionService.GetCreatorId());
            CreatorViewModel creator = await _mediator.Send(query);
            return Ok(creator);
        }

        [Authorize(Policy = AuthorizationPolicies.CAN_ESCALATE)]
        [HttpPut("me")]
        public async Task<ActionResult<CreatorViewModel>> UpdateByCurrentSession([FromBody] UpdateCreatorDto dto)
        {
            int creatorId = await GetValidatedCreatorIdAsync(_sessionService.GetUserId());
            UpdateCreatorCommand command = new(
                id: creatorId,
                userId: _sessionService.GetUserId(),
                dto: dto
            );
            CreatorViewModel updatedCreator = await _mediator.Send(command);
            return Ok(updatedCreator);
        }

        private async Task<int> GetValidatedCreatorIdAsync(string userId)
        {
            AppUser? user = await _userManager.FindByIdAsync(userId)
                ?? throw new NotFoundException("AppUSer", userId);

            if (!user.CreatorId.HasValue)
                throw new InvalidOperationException("The current user doesn't have a configured creator.");

            return user.CreatorId.Value;
        }
    }
}
