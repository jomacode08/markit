using FluentValidation;

namespace markit.Application.Features.Settings.Commands.UpdateDemoSettings
{
    public class UpdateDemoSettingsCommandValidator : AbstractValidator<UpdateDemoSettingsCommand>
    {
        public UpdateDemoSettingsCommandValidator() {
            RuleFor(c => c.IsEnabled).NotNull().WithMessage("The isEnabled field is required.");
            RuleFor(c => c.UserId).NotEmpty().WithMessage("The userId field is required.");
            RuleFor(c => c.TokenDurationInMinutes)
                .NotEmpty().WithMessage("The tokenDurationInMinutes field is required.")
                .GreaterThan(0).WithMessage("The tokenDurationInMinutes must be greater than zero.");
        }
    }
}
