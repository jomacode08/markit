using FluentValidation;

namespace markit.Application.Features.Marks.Commands.DeleteMarkCommand
{
    public class SoftDeleteMarkCommandValidator : AbstractValidator<SoftDeleteMarkCommand>
    {
        public SoftDeleteMarkCommandValidator()
        {
            RuleFor(c => c.Id).NotEmpty().WithMessage("The id is required");
        }
    }
}
