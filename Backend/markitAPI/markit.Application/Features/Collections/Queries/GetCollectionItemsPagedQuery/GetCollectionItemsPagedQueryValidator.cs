using FluentValidation;

namespace markit.Application.Features.Collections.Queries.GetCollectionItemsPagedQuery
{
    public class GetCollectionItemsPagedQueryValidator : AbstractValidator<GetCollectionItemsPagedQuery>
    {
        public GetCollectionItemsPagedQueryValidator() {
            RuleFor(q => q.CreatorId).NotEmpty();
            RuleFor(q => q.PageSize).NotEmpty();
            RuleFor(q => q.Filters).NotEmpty();
            RuleFor(q => q.Filters.Type).IsInEnum();
            RuleFor(q => q.SortOrder).IsInEnum();
        }
    }
}
