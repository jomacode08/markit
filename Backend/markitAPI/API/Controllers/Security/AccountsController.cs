using markit.API.Controllers.Common;
using markit.Application.Features.Accounts.Commands.UpdateAccount;
using markit.Application.Features.Accounts.Queries;
using markit.Application.Models.Authentication.AppUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static markit.Application.Helpers.GeneralConstant;

namespace markit.API.Controllers.Security
{
    [Authorize(Policy = AuthorizationPolicies.ADMIN_ONLY)]
    public class AccountsController : ApiControllerBase
    {
        private readonly IMediator _mediator;

        public AccountsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Route("all")]
        public async Task<ActionResult<AppUserPaginationDto>> GetAllPaged(int page, int limit)
        {
            GetAccountsPagedQuery query = new(
                pageIndex: page,
                pageSize: limit
            );
            return Ok(await _mediator.Send(query));
        }

        [HttpPut]
        [Route("{userId}")]
        public async Task<ActionResult> UpdateAccount([FromRoute] string  userId, [FromBody]UpdateAccountCommandDto dto)
        {
            UpdateAccountCommand command = new(userId, dto);
            await _mediator.Send(command);
            return Ok();
        }
    }
}
