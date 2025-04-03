using FluentValidation;

namespace markit.Application.Features.Collections.Commands.UpdateCollectionCommand
{
    public class UpdateCollectionCommandValidator : AbstractValidator<UpdateCollectionCommand>
    {
        public UpdateCollectionCommandValidator() {
            RuleFor(c => c.Id).NotEmpty().WithMessage("The Id property is required");
            RuleFor(c => c.Name).NotEmpty().WithMessage("The Id property is required");
            RuleFor(c => c.Name).MaximumLength(255).WithMessage("The maximum length of name property is 255 characters");
        }
    }
}
