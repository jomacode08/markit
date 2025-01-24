using FluentValidation;

namespace markit.Application.Features.Marks.Commands.UpdateMarkCommand
{
    public class UpdateMarkCommandValidator : AbstractValidator<UpdateMarkCommand>
    {
        public UpdateMarkCommandValidator()
        {
            RuleFor(c => c.Name).NotEmpty().WithMessage("The name is required");
            RuleFor(c => c.Name).MaximumLength(255).WithMessage("The maximun length of the name title is 255");
            RuleFor(c => c.Id).NotEmpty().WithMessage("The id is required");
        }
    }
}
