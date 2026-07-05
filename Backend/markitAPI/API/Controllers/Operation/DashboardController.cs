using markit.API.Controllers.Common;
using markit.Application.Features.Reports.Dashboard.Queries;
using markit.Application.Features.Reports.Dashboard.Queries.ViewModels;
using markit.Infrastructure.Security.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace markit.API.Controllers.Operation
{
    [Authorize]
    public class DashboardController : ApiControllerBase
    {
        private readonly IMediator _mediator;
        private readonly SessionService _sessionService;

        public DashboardController(IMediator mediator, SessionService sessionService)
        {
            _mediator = mediator;
            _sessionService = sessionService;
        }

        [HttpGet]
        public async Task<ActionResult<DashboardReportVm>> GetReportByCurrentSession([FromQuery]string clientTimeZone)
        {
            DashboardReportQuery query = new(userId: _sessionService.GetUserId(), clientTimeZone);
            return Ok(await _mediator.Send(query));
        }
    }
}
