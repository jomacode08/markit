using markit.Application.Common.Helpers;
using markit.Application.Contracts.Settings;
using markit.Application.Exceptions;
using markit.Application.Features.Settings.Queries.ViewModels;
using markit.Application.Models.Authentication.Enums;
using markit.Application.Models.Authentication.GitHub;
using markit.Application.Models.Authentication.Google;
using markit.Application.Models.Settings;
using MediatR;
using Microsoft.Extensions.Options;
using System.Transactions;
using static markit.Application.Helpers.GeneralConstant;

namespace markit.Application.Features.Settings.Commands.UpdateExternalAuthSettings
{
    public class UpdateExternalAuthSettingsCommand : IRequest<ExternalAuthSettings>
    {
        public bool IsGoogleEnabled { get; init; }
        public bool IsGitHubEnabled { get; init; }
    }

    public class UpdateExternalAuthSettingsCommandHandler : IRequestHandler<UpdateExternalAuthSettingsCommand, ExternalAuthSettings>
    {
        private readonly ISettingsService _settingsService;
        private readonly GoogleAuthSettings _googleAuthSettings;
        private readonly GitHubAuthSettings _gitHubAuthSettings;

        public UpdateExternalAuthSettingsCommandHandler(
            ISettingsService settingsService,
            IOptions<GitHubAuthSettings> gitHubAuthSettings,
            IOptions<GoogleAuthSettings> googleAuthSettings
        )
        {
            _settingsService = settingsService;
            _gitHubAuthSettings = gitHubAuthSettings.Value;
            _googleAuthSettings = googleAuthSettings.Value;
        }

        public async Task<ExternalAuthSettings> Handle(UpdateExternalAuthSettingsCommand request, CancellationToken cancellationToken)
        {
            // Start transaction
            using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);
            
            await UpdateSettingAsync(
                key: SystemConfigKeys.AUTH_IS_GOOGLE_ENABLED_KEY,
                value: request.IsGoogleEnabled
            );

            await UpdateSettingAsync(
                key: SystemConfigKeys.AUTH_IS_GITHUB_ENABLED_KEY,
                value: request.IsGitHubEnabled
            );

            // End transaction
            scope.Complete();
            return new ExternalAuthSettings(
                request.IsGoogleEnabled,
                request.IsGitHubEnabled
            );
        }

        private async Task<SystemConfig> UpdateSettingAsync(string key, bool value)
        {
            ValidateLoginProviderConfigurationScheme(key, value);
            return await _settingsService.UpdateAsync(key, value.ToString());
        }

        private void ValidateLoginProviderConfigurationScheme(string key, bool value)
        {
            if (value && !IsLoginProviderSchemeConfigured(key))
            {
                LoginProvider loginProvider = key switch
                {
                    SystemConfigKeys.AUTH_IS_GOOGLE_ENABLED_KEY => LoginProvider.Google,
                    SystemConfigKeys.AUTH_IS_GITHUB_ENABLED_KEY => LoginProvider.GitHub,
                    _ => throw new ArgumentOutOfRangeException(paramName: nameof(key))
                };

                throw new CustomValidationException(GetLoginProviderSchemeErrorMessage(loginProvider));
            }
        }

        private bool IsLoginProviderSchemeConfigured(string key)
        {
            return key switch
            {
                SystemConfigKeys.AUTH_IS_GOOGLE_ENABLED_KEY => _googleAuthSettings.IsConfigured(),
                SystemConfigKeys.AUTH_IS_GITHUB_ENABLED_KEY => _gitHubAuthSettings.IsConfigured(),
                _ => throw new ArgumentOutOfRangeException(paramName: nameof(key))
            };
        }

        private static string GetLoginProviderSchemeErrorMessage(LoginProvider provider) => $"Cannot enable {provider.GetName()} because its credentials are not configured. Update the configuration scheme to proceed.";
        private static string GetBadConfigurationFormatErrorMessage(string key) => $"The configuration: {key} has a bad format.";
    }
}
