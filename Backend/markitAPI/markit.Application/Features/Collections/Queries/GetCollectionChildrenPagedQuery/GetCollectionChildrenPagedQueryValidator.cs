using FluentValidation;

namespace markit.Application.Features.Collections.Queries.GetCollectionChildrenPagedQuery
{
    public class GetCollectionChildrenPagedQueryValidator : AbstractValidator<GetCollectionChildrenPagedQuery>
    {
        public GetCollectionChildrenPagedQueryValidator() {
            RuleFor(q => q.CollectionId).NotEmpty();
            RuleFor(q => q.CreatorId).NotEmpty();
            RuleFor(q => q.Page).NotEmpty();
            RuleFor(q => q.PageSize).NotEmpty();
            RuleFor(q => q.CollectionItemCategory).NotNull();
        }
    }
}
