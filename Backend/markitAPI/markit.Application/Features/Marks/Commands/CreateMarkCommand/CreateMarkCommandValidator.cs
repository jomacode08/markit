using FluentValidation;

namespace markit.Application.Features.Marks.Commands.CreateMarkCommand
{
    public class CreateMarkCommandValidator : AbstractValidator<CreateMarkCommand>
    {
        public CreateMarkCommandValidator() {
            RuleFor(c => c.Name).NotEmpty().WithMessage("The name is required");
            RuleFor(c => c.Name).MaximumLength(255).WithMessage("The maximun length of the name title is 255");
            RuleFor(c => c.CreatorId).NotEmpty().WithMessage("The creator id is required");
        }
    }
}
