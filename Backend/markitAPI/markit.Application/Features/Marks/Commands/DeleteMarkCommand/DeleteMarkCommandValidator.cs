using FluentValidation;

namespace markit.Application.Features.Marks.Commands.DeleteMarkCommand
{
    public class DeleteMarkCommandValidator : AbstractValidator<DeleteMarkCommand>
    {
        public DeleteMarkCommandValidator()
        {
            RuleFor(c => c.Id).NotEmpty().WithMessage("The id is required");
        }
    }
}
