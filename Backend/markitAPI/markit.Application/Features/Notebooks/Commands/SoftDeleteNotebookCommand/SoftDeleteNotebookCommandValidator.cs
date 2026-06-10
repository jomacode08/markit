using FluentValidation;

namespace markit.Application.Features.Notebooks.Commands.DeleteNotebookCommand
{
    public class SoftDeleteNotebookCommandValidator : AbstractValidator<SoftDeleteNotebookCommand>
    {
        public SoftDeleteNotebookCommandValidator()
        {
            RuleFor(c => c.Id).NotEmpty().WithMessage("The id is required");
        }
    }
}
