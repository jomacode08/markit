using FluentValidation;

namespace markit.Application.Features.Accounts.Queries
{
    public class GetAccountsPagedQueryValidator : AbstractValidator<GetAccountsPagedQuery>
    {
        public GetAccountsPagedQueryValidator() {
            RuleFor(q => q.PageIndex).NotNull().WithMessage("The pageIndex is required.");
            RuleFor(q => q.PageSize).NotNull().WithMessage("The pageSize is required.");
            RuleFor(q => q.PageIndex).GreaterThan(0).WithMessage("The pageIndex must be greater than cero.");
            RuleFor(q => q.PageSize).GreaterThan(0).WithMessage("The pageSize must be greater than cero."); ;
        }
    }
}
