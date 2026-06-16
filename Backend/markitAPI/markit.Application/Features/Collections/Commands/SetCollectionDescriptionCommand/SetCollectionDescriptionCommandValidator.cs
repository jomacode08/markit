using FluentValidation;

namespace markit.Application.Features.Collections.Commands.SetCollectionDescriptionCommand
{
    public class SetCollectionDescriptionCommandValidator : AbstractValidator<SetCollectionDescriptionCommand>
    {
        public SetCollectionDescriptionCommandValidator() {
            RuleFor(c => c.Id).NotEmpty().WithMessage("The id is required");
            RuleFor(c => c.UserId).NotEmpty().WithMessage("The userId is required");
            RuleFor(c => c.Description).MaximumLength(1500).WithMessage("The maximum length of description field is 1500 characters");            RuleFor(c => c.Description).MaximumLength(1500).WithMessage("The maximum length of description field is 1500 characters");
        }
    }
}
