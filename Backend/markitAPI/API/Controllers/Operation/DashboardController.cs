using markit.Application.Features.Reports.Dashboard.Queries;
using markit.Application.Features.Reports.Dashboard.Queries.ViewModels;
using markit.Infraestructure.Security.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace markit.API.Controllers.Operation
{
    [Authorize]
    [Route("api/dashboard")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly SessionService _sessionService;

        public DashboardController(IMediator mediator, SessionService sessionService)
        {
            _mediator = mediator;
            _sessionService = sessionService;
        }

        [HttpGet]
        [Route("getReportByCurrentSession")]
        public async Task<DashboardReportVm> GetReportByCurrentSession()
        {
            DashboardReportQuery query = new()
            {
                CreatorId = _sessionService.GetCreatorId()
            };
            return await _mediator.Send(query);
        }
    }
}
