using markit.Application.Contracts.Authentication;
using markit.Application.Models.Authentication.AppUser;
using MediatR;

namespace markit.Application.Features.Accounts.Queries
{
    public class GetAccountsPagedQuery(int pageIndex, int pageSize) : IRequest<AppUserPaginationDto>
    {
        public int PageIndex { get; } = pageIndex;
        public int PageSize { get; } = pageSize;
    }

    public class GetAccountsPagedQueryHandler : IRequestHandler<GetAccountsPagedQuery, AppUserPaginationDto>
    {
        private readonly IAppUserService _appUserservice;

        public GetAccountsPagedQueryHandler(IAppUserService appUserservice)
        {
            _appUserservice = appUserservice;
        }

        public Task<AppUserPaginationDto> Handle(GetAccountsPagedQuery request, CancellationToken cancellationToken)
        {
            return _appUserservice.GetPagedAsync(
                page: request.PageIndex,
                pageSize: request.PageSize
            );
        }
    }
}
