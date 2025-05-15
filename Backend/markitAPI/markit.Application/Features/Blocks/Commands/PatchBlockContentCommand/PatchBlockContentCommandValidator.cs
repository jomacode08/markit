using FluentValidation;
using markit.Domain.Entities;

namespace markit.Application.Features.Blocks.Commands.PatchBlockContentCommand
{
    class PatchBlockContentCommandValidator : AbstractValidator<Block>
    {
        public PatchBlockContentCommandValidator()
        {
            RuleFor(b => b.Id).NotEmpty().WithMessage("The id is required");
            RuleFor(b => b.Content).MaximumLength(255).WithMessage("The maximum length of content property is 255 characters");
        }
    }
}
