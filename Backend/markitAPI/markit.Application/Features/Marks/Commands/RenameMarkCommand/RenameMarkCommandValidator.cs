using FluentValidation;

namespace markit.Application.Features.Marks.Commands.RenameMarkCommand
{
    public class RenameMarkCommandValidator : AbstractValidator<RenameMarkCommand>
    {
        public RenameMarkCommandValidator() {
            RuleFor(c => c.Id).NotEmpty().WithMessage("The id is required");
            RuleFor(c => c.UserId).NotEmpty().WithMessage("The userId is required");
            RuleFor(c => c.Name).NotEmpty().WithMessage("The name is required");
            RuleFor(c => c.Name).MaximumLength(255).WithMessage("The maximum length of the name is 255 characters");
        }
    }
}
