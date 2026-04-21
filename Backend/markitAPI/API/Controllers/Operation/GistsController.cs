using markit.API.Controllers.Common;
using markit.Application.Exceptions;
using markit.Application.Features.Gists.Queries;
using markit.Application.Features.Gists.Queries.ViewModels;
using markit.Application.Models.Authentication.AppUser;
using markit.infrastructure.Security.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace markit.API.Controllers.Operation
{
    [Authorize]
    public class GistsController : ApiControllerBase
    {
        private readonly IMediator _mediator;
        private readonly SessionService _sessionService;
        private readonly UserManager<AppUser> _userMannager;

        public GistsController(IMediator mediator, SessionService sessionService, UserManager<AppUser> userMannager)
        {
            _mediator = mediator;
            _sessionService = sessionService;
            _userMannager = userMannager;
        }

        [HttpGet("{id}")]
        public async Task<GistResponse> GetById(string id)
        {
            string userId = _sessionService.GetUserId();
            AppUser user = await _userMannager.FindByIdAsync(userId)
                ?? throw new NotFoundException("Users", userId);

            GetGistByIdQuery query = new()
            {
                Id = id,
                User = user,
            };

            return await _mediator.Send(query);
        }
    }
}
