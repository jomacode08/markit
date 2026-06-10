using FluentValidation;

namespace markit.Application.Features.Notebooks.Commands.UpdateNotebookCommand
{
    public class UpdateNotebookCommandValidator : AbstractValidator<UpdateNotebookCommand>
    {
        public UpdateNotebookCommandValidator()
        {
            RuleFor(c => c.InputName).MaximumLength(255).WithMessage("The maximun length of the name title is 255");
            RuleFor(c => c.Id).NotEmpty().WithMessage("The id is required");
            RuleFor(c => c.Blocks).NotEmpty().WithMessage("The notebook should contain at least one block");
        }
    }
}
