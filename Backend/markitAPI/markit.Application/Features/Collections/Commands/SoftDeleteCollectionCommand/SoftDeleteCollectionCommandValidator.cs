using FluentValidation;

namespace markit.Application.Features.Collections.Commands.DeleteCollectionCommand
{
    public class SoftDeleteCollectionCommandValidator : AbstractValidator<SoftDeleteCollectionCommand>
    {
        public SoftDeleteCollectionCommandValidator() {
            RuleFor(c => c.CollectionId).NotEmpty().WithMessage("The collectionId is required");
        }
    }
}
