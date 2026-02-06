using FluentValidation;
using static markit.Application.Helpers.GeneralConstant;

namespace markit.Application.Features.Accounts.Commands.UpdateAccount
{
    public class UpdateAccountCommandValidator : AbstractValidator<UpdateAccountCommand>
    {
        public UpdateAccountCommandValidator() {
            RuleFor(c => c.UserId).NotEmpty().WithMessage("The userId is required");
            RuleFor(c => c.FirstName).NotEmpty().WithMessage("The firstName is required");
            RuleFor(c => c.LastName).NotEmpty().WithMessage("The lastName is required");
            RuleFor(c => c.Email).NotEmpty().WithMessage("The email is required");
            RuleFor(c => c.Roles).NotEmpty().WithMessage("The account must have at least one role");

            string nameFormatRegex = "^[a-zA-Z ]+$";
            RuleFor(c => c.FirstName).Matches(nameFormatRegex)
                .WithMessage("The first name format is incorrect");
            RuleFor(c => c.LastName).Matches(nameFormatRegex)
                .WithMessage("The last name format is incorrect");
            RuleFor(c => c.Email).Matches(@"^[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}$")
                .WithMessage("The email format is incorrect");

            RuleFor(c => c.FirstName).MaximumLength(100).WithMessage("The maximun length of first name is 100");
            RuleFor(c => c.LastName).MaximumLength(100).WithMessage("The maximun length of last name is 100");
            RuleFor(c => c.Email).MaximumLength(255).WithMessage("The maximun length of last name is 256");

            RuleForEach(c => c.Roles)
                .Must(r => Role.All.Contains(r))
                .WithMessage(r => $"The rol {r} is not permitted.");
        }
    }
}
