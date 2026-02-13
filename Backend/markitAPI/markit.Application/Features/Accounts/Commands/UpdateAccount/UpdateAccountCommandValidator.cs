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
            RuleFor(c => c.UserName).NotEmpty().WithMessage("The userName is required");
            RuleFor(c => c.Enabled).NotNull().WithMessage("The enabled field is required");
            RuleFor(c => c.Roles).NotEmpty().WithMessage("The account must have at least one role");

            string nameFormatRegex = "^[a-zA-Z ]+$";
            RuleFor(c => c.FirstName).Matches(nameFormatRegex)
                .WithMessage("The first name format is incorrect");
            RuleFor(c => c.LastName).Matches(nameFormatRegex)
                .WithMessage("The last name format is incorrect");
            RuleFor(c => c.UserName).Matches(@"^[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}$")
                .WithMessage("The user name format is incorrect");

            RuleFor(c => c.FirstName).MaximumLength(100).WithMessage("The maximun length of the first name field is 100");
            RuleFor(c => c.LastName).MaximumLength(100).WithMessage("The maximun length of the last name field is 100");
            RuleFor(c => c.UserName).MaximumLength(256).WithMessage("The maximun length of the userName field is 256");

            RuleForEach(c => c.Roles)
                .Must(r => Role.All.Contains(r))
                .WithMessage((c, r) => $"The role {r} is not permitted.");
        }
    }
}
