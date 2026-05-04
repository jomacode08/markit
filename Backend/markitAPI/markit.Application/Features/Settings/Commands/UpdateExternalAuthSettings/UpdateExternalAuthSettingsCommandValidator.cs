using FluentValidation;

namespace markit.Application.Features.Settings.Commands.UpdateExternalAuthSettings
{
    public class UpdateExternalAuthSettingsCommandValidator : AbstractValidator<UpdateExternalAuthSettingsCommand>
    {
        public UpdateExternalAuthSettingsCommandValidator() {
            RuleFor(c => c.IsGoogleEnabled).NotNull().WithMessage("The IsGoogleEnabled field is required.");
            RuleFor(c => c.IsGitHubEnabled).NotNull().WithMessage("The IsGitHubEnabled field is required.");
        }
    }
}
