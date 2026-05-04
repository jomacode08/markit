using markit.Application.Contracts.Settings;
using markit.Application.Features.Settings.Queries.ViewModels;
using MediatR;
using static markit.Application.Helpers.GeneralConstant;

namespace markit.Application.Features.Settings.Queries
{
    public class GetExternalAuthSettingsQuery : IRequest<ExternalAuthSettings> {}
    public class GetExternalAuthSettingsQueryHandler : IRequestHandler<GetExternalAuthSettingsQuery, ExternalAuthSettings>
    {
        private readonly ISettingsService _settingsService;

        public GetExternalAuthSettingsQueryHandler(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public async Task<ExternalAuthSettings> Handle(GetExternalAuthSettingsQuery request, CancellationToken cancellationToken)
        {
            bool isGoogleEnabled = await GetSettingAsync(SystemConfigKeys.AUTH_IS_GOOGLE_ENABLED_KEY);
            bool isGitHubEnabled = await GetSettingAsync(SystemConfigKeys.AUTH_IS_GITHUB_ENABLED_KEY);

            return new ExternalAuthSettings(
                isGoogleEnabled,
                isGitHubEnabled
            );
        }

        private async Task<bool> GetSettingAsync(string key)
        {
            string? value = await _settingsService.GetValueAsync(key);
            if (value == null || !bool.TryParse(value, out bool result)) return false;
            return result;
        }
    }
}
