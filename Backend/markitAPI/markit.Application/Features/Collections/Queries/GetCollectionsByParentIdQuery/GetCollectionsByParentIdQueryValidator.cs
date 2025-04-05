using FluentValidation;

namespace markit.Application.Features.Collections.Queries.GetCollectionsByParentIdQuery
{
    public class GetCollectionsByParentIdQueryValidator : AbstractValidator<GetCollectionsByParentIdQuery>
    {
        public GetCollectionsByParentIdQueryValidator() {
            RuleFor(q => q.ParentId).NotEmpty().WithMessage("The parentId is required.");
        }
    }
}
