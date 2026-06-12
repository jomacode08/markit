using FluentValidation;
using markit.Application.Common.Helpers;

namespace markit.Application.Features.Accounts.Commands.RenameAccount
{
    public class RenameAccountCommandValidator : AbstractValidator<RenameAccountCommand>
    {
        public RenameAccountCommandValidator()
        {
            RuleFor(c => c.UserId).NotEmpty().WithMessage("The userId is required");
            RuleFor(c => c.Name).NotEmpty().WithMessage("The name is required");
            RuleFor(c => c.UserName).NotEmpty().WithMessage("The userName is required");

            RuleFor(c => c.UserName).Matches(ValidationRegexes.EmailRegex)
                .WithMessage("The user name format is incorrect");

            RuleFor(c => c.Name).Matches(ValidationRegexes.InternationalNameRegex)
                .WithMessage("The name format is invalid");

            RuleFor(c => c.Name).MaximumLength(50).WithMessage("The maximum length of the name field is 50");
            RuleFor(c => c.UserName).MaximumLength(256).WithMessage("The maximum length of the userName field is 256");
        }
    }
}
