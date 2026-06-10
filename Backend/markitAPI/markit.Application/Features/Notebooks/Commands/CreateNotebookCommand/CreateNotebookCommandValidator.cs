using FluentValidation;

namespace markit.Application.Features.Notebooks.Commands.CreateNotebookCommand
{
    public class CreateNotebookCommandValidator : AbstractValidator<CreateNotebookCommand>
    {
        public CreateNotebookCommandValidator()
        {
            RuleFor(c => c.Name).NotEmpty().WithMessage("The name is required");
            RuleFor(c => c.Name).MaximumLength(255).WithMessage("The maximun length of the name title is 255");
            RuleFor(c => c.UserId).NotEmpty().WithMessage("The userId is required");
            RuleFor(c => c.Blocks).NotEmpty().WithMessage("Blocks are required");

            RuleForEach(c => c.Blocks).ChildRules(b => {
                b.RuleFor(b => b.Title).NotEmpty();
            });
        }
    }
}
