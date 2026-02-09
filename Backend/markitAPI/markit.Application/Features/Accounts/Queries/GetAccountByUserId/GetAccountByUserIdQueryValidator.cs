
using FluentValidation;

namespace markit.Application.Features.Accounts.Queries.GetAccountByUserId
{
    public class GetAccountByUserIdQueryValidator : AbstractValidator<GetAccountByUserIdQuery>
    {
        public GetAccountByUserIdQueryValidator()
        {
            RuleFor(c => c.UserId).NotEmpty().WithMessage("The userId is required.");
        }
    }
}
