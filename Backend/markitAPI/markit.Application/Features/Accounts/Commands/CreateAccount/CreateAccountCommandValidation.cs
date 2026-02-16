using FluentValidation;
using static markit.Application.Helpers.GeneralConstant;

namespace markit.Application.Features.Account.Commands.CreateAccount
{
    public class CreateAccountCommandValidation : AbstractValidator<CreateAccountCommand>
    {
        public CreateAccountCommandValidation()
        {
            RuleFor(c => c.FirstName).NotEmpty().WithMessage("The firstName is required");
            RuleFor(c => c.LastName).NotEmpty().WithMessage("The lastName is required");
            RuleFor(c => c.UserName).NotEmpty().WithMessage("The userName is required");
            RuleFor(c => c.AccessType).NotEmpty().WithMessage("The accessType is required");
            RuleFor(c => c.Enabled).NotNull().WithMessage("The enabled field is required");
            RuleFor(c => c.Roles).NotEmpty().WithMessage("The account must have at least one role");

            RuleFor(c => c.UserName).Matches(@"^[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}$")
                .WithMessage("The user name format is incorrect");

            RuleFor(c => c.FirstName).MaximumLength(100).WithMessage("The maximun length of the first name field is 100");
            RuleFor(c => c.LastName).MaximumLength(100).WithMessage("The maximun length of the last name field is 100");
            RuleFor(c => c.UserName).MaximumLength(256).WithMessage("The maximun length of the user name field is 256");

            RuleForEach(c => c.Roles)
                .Must(r => Role.All.Contains(r))
                .WithMessage((c, r) => $"The role {r} is not permitted.");
        }
    }
}
