using markit.API.Controllers.Common;
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
        public async Task<ActionResult<AppUserPaginationDto>> GetAllPaged(int page, int limit)
        {
            GetAccountsPagedQuery query = new(
                pageIndex: page,
                pageSize: limit
            );
            return Ok(await _mediator.Send(query));
        }
    }
}
