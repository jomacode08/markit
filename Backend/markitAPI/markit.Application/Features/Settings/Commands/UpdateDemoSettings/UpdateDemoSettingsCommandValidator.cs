using FluentValidation;

namespace markit.Application.Features.Settings.Commands.UpdateDemoSettings
{
    public class UpdateDemoSettingsCommandValidator : AbstractValidator<UpdateDemoSettingsCommand>
    {
        public UpdateDemoSettingsCommandValidator() {
            RuleFor(c => c.IsEnabled).NotNull().WithMessage("The isEnabled field is required.");
            RuleFor(c => c.UserTemplateId)
                .NotNull().WithMessage("The userTemplateId field is required.")
                .Must(id => string.IsNullOrEmpty(id) || Guid.TryParse(id, out _))
                .WithMessage("The userTemplateId must be a valid UUID or empty.");
            RuleFor(c => c.SessionDurationInMinutes)
                .NotEmpty().WithMessage("The sessionDurationInMinutes field is required.")
                .GreaterThan(0).WithMessage("The sessionDurationInMinutes must be greater than zero.");
        }
    }
}
