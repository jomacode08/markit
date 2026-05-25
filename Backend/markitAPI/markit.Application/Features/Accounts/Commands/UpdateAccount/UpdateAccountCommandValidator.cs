using FluentValidation;
using static markit.Application.Helpers.GeneralConstant;

namespace markit.Application.Features.Accounts.Commands.UpdateAccount
{
    public class UpdateAccountCommandValidator : AbstractValidator<UpdateAccountCommand>
    {
        public UpdateAccountCommandValidator() {
            RuleFor(c => c.UserId).NotEmpty().WithMessage("The userId is required");
            RuleFor(c => c.Name).NotEmpty().WithMessage("The name is required");
            RuleFor(c => c.UserName).NotEmpty().WithMessage("The userName is required");
            RuleFor(c => c.Enabled).NotNull().WithMessage("The enabled field is required");
            RuleFor(c => c.Roles).NotEmpty().WithMessage("The account must have at least one role");

            RuleFor(c => c.UserName).Matches(@"^[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}$")
                .WithMessage("The user name format is incorrect");

            RuleFor(c => c.Name).MaximumLength(256).WithMessage("The maximum length of the name field is 256");
            RuleFor(c => c.UserName).MaximumLength(256).WithMessage("The maximum length of the userName field is 256");

            RuleForEach(c => c.Roles)
                .Must(r => Role.All.Contains(r))
                .WithMessage((c, r) => $"The role {r} is not permitted.");
        }
    }
}
