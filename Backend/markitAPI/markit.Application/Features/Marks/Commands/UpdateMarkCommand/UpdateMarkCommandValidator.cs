using FluentValidation;

namespace markit.Application.Features.Marks.Commands.UpdateMarkCommand
{
    public class UpdateMarkCommandValidator : AbstractValidator<UpdateMarkCommand>
    {
        public UpdateMarkCommandValidator()
        {
            RuleFor(c => c.InputName).MaximumLength(255).WithMessage("The maximun length of the name title is 255");
            RuleFor(c => c.Id).NotEmpty().WithMessage("The id is required");
            RuleFor(c => c.Blocks).NotEmpty().WithMessage("The mark should contain at least one block");
        }
    }
}
