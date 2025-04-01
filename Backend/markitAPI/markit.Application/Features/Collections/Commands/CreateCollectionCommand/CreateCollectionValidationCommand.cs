using FluentValidation;

namespace markit.Application.Features.Collections.Commands.CreateCollectionCommand
{
    public class CreateCollectionValidationCommand : AbstractValidator<CreateCollectionCommand>
    {
        public CreateCollectionValidationCommand() {
            RuleFor(c => c.Name).NotEmpty().WithMessage("The name property is required");
            RuleFor(c => c.Name).MaximumLength(255).WithMessage("The maximum length of name property is 255 characters");
            RuleFor(c => c.IsMain).NotNull().WithMessage("The isMain property is required");
            RuleFor(c => c.CreatorId).NotEmpty().WithMessage("The creatorId porperty is required");
        }
    }
}
