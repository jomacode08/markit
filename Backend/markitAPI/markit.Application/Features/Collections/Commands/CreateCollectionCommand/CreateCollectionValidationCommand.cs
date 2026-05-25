using FluentValidation;

namespace markit.Application.Features.Collections.Commands.CreateCollectionCommand
{
    public class CreateCollectionValidationCommand : AbstractValidator<CreateCollectionCommand>
    {
        public CreateCollectionValidationCommand() {
            RuleFor(c => c.Name).NotEmpty().WithMessage("The name field is required");
            RuleFor(c => c.Name).MaximumLength(255).WithMessage("The maximum length of name field is 255 characters");
            RuleFor(c => c.IsMain).NotNull().WithMessage("The isMain field is required");
            RuleFor(c => c.UserId).NotEmpty().WithMessage("The userId field is required");
        }
    }
}
