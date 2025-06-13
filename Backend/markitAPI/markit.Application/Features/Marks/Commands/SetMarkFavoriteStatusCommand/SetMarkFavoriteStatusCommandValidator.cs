using FluentValidation;

namespace markit.Application.Features.Marks.Commands.SetMarkFavoriteStatusCommand
{
    public class SetMarkFavoriteStatusCommandValidator : AbstractValidator<SetMarkFavoriteStatusCommand>
    {
        public SetMarkFavoriteStatusCommandValidator() { 
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.CreatorId).NotEmpty();
            RuleFor(x => x.IsFavorite).NotNull();
        }
    }
}
