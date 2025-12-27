using FluentValidation;

namespace markit.Application.Features.Collections.Queries.GetCollectionItemsPagedQuery
{
    public class GetCollectionItemsPagedQueryValidator : AbstractValidator<GetCollectionItemsPagedQuery>
    {
        public GetCollectionItemsPagedQueryValidator() {
            RuleFor(q => q.PaginationRequest.CreatorId).NotEmpty();
            RuleFor(q => q.PaginationRequest.PageSize).NotEmpty();
            RuleFor(q => q.PaginationRequest.Filters).NotEmpty();
            RuleFor(q => q.PaginationRequest.Filters.Type).IsInEnum();
            RuleFor(q => q.PaginationRequest.SortOrder).IsInEnum();
        }
    }
}
