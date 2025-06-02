using FluentValidation;

namespace markit.Application.Features.Collections.Queries.GetCollectionByIdQuery
{
    public class GetcollectionByIdQueryValidator : AbstractValidator<GetCollectionByIdQuery>
    {
        public GetcollectionByIdQueryValidator() {
            RuleFor(q => q.CollectionId).NotEmpty().WithMessage("The collectionId is required.");
        }
    }
}
