using FluentValidation;

namespace markit.Application.Features.Notebooks.Commands.SetNotebookFavoriteStatusCommand
{
    public class SetNotebookFavoriteStatusCommandValidator : AbstractValidator<SetNotebookFavoriteStatusCommand>
    {
        public SetNotebookFavoriteStatusCommandValidator() { 
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.IsFavorite).NotNull();
        }
    }
}
