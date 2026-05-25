﻿using markit.API.Controllers.Common;
using markit.Application.Features.Account.Commands.CreateAccount;
using markit.Application.Features.Accounts.Commands.CreateAccount;
using markit.Application.Features.Accounts.Commands.UpdateAccount;
using markit.Application.Features.Accounts.Queries;
using markit.Application.Features.Accounts.Queries.GetAccountByUserId;
using markit.Application.Features.Accounts.Queries.ViewModels;
using markit.Application.Models.Authentication.AppUser;
using markit.Application.Models.Authentication.Enums;
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

        [HttpGet]
        [Route("{userId}")]
        public async Task<ActionResult<AccountVm>> GetByUserId([FromRoute] string userId)
        {
            GetAccountByUserIdQuery query = new(userId);
            return Ok(await _mediator.Send(query));
        }
        
        [HttpPost]
        public async Task<ActionResult<AccountVm>> Create([FromBody] CreateAccountCommandDto dto)
        {
            CreateAccountCommand command = new()
            {
                Name = dto.Account.Name,
                UserName = dto.Account.UserName,
                Roles = dto.Account.Roles,
                Password = dto.Password.NewPassword,
                Enabled = dto.Account.Enabled,
                AccessType = AccessType.Internal,
            };

            AccountVm result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPut]
        [Route("{userId}")]
        public async Task<ActionResult<AccountVm>> Update([FromRoute] string userId, [FromBody] UpdateAccountCommandDto dto)
        {
            UpdateAccountCommand command = new(userId, dto);
            AccountVm result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}
