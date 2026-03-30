using FluentValidation;

namespace markit.Application.Features.Marks.Commands.MoveMarkCommand
{
    public class MoveMarkCommandValidator : AbstractValidator<MoveMarkCommand>
    {
        public MoveMarkCommandValidator() {
            RuleFor(c => c.MarkId).NotEmpty().WithMessage("The markId is required");
            RuleFor(c => c.CollectionId).NotEmpty().WithMessage("The collectionId is required");
            RuleFor(c => c.CreatorId).NotEmpty().WithMessage("The creatorId is required");
        }
    }
}
