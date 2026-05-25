using FluentValidation;

namespace markit.Application.Features.Collections.Commands.SetCollectionFavoriteStatusCommand
{
    public class SetCollectionFavoriteStatusCommandValidator : AbstractValidator<SetCollectionFavoriteStatusCommand>
    {
        public SetCollectionFavoriteStatusCommandValidator() { 
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.IsFavorite).NotNull();
        }
    }
}
