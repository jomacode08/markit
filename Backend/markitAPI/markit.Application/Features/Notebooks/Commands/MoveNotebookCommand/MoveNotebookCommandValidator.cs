using FluentValidation;

namespace markit.Application.Features.Notebooks.Commands.MoveNotebookCommand
{
    public class MoveNotebookCommandValidator : AbstractValidator<MoveNotebookCommand>
    {
        public MoveNotebookCommandValidator() {
            RuleFor(c => c.NotebookId).NotEmpty().WithMessage("The notebookId is required");
            RuleFor(c => c.CollectionId).NotEmpty().WithMessage("The collectionId is required");
            RuleFor(c => c.UserId).NotEmpty().WithMessage("The userId is required");
        }
    }
}