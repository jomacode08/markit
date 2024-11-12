
using FluentValidation;

namespace markit.Application.Features.Creators.Commands.CreateCreator
{
    public class CreateCreatorCommandValidator : AbstractValidator<CreateCreatorCommand>
    {
        public CreateCreatorCommandValidator() 
        {
            RuleFor(c => c.FirstName).NotEmpty().WithMessage("The first name is required");
            RuleFor(c => c.LastName).NotEmpty().WithMessage("The first name is required");
        }
    }
}
