using markit.Application.Contracts.Settings;
using markit.Application.Features.Settings.Queries.ViewModels;
using markit.Application.Models.Settings;
using MediatR;
using static markit.Application.Helpers.GeneralConstant;

namespace markit.Application.Features.Settings.Queries
{
    public class GetDemoSettingsQuery : IRequest<DemoSettings> {}

    public class GetDemoSettingsQueryHandler : IRequestHandler<GetDemoSettingsQuery, DemoSettings>
    {
        private readonly ISettingsService _settingsService;

        public GetDemoSettingsQueryHandler(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public async Task<DemoSettings> Handle(GetDemoSettingsQuery request, CancellationToken cancellationToken)
        {
            IReadOnlyList<SystemConfig> settings = await _settingsService.GetForDemoAsync();
            string? isEnabledValue = GetSettingValue(SystemConfigKeys.IS_DEMO_ENABLED_KEY, settings);
            string? sessionDurationInMinutesValue = GetSettingValue(SystemConfigKeys.DEMO_SESSION_DURATION_IN_MINUTES, settings);

            return ConstructDemoSettings(
                isEnabledValue,
                sessionDurationInMinutesValue
            );
        }

        private static string? GetSettingValue(string key, IReadOnlyList<SystemConfig> settings)
        {
            return settings.FirstOrDefault(s => s.Id.Equals(key, StringComparison.OrdinalIgnoreCase))?.Value;
        }

        private static DemoSettings ConstructDemoSettings(
            string? isEnabledValue,
            string? sessionDurationInMinutesValue
        ) {
            DemoSettings demoSettings = new();

            if (bool.TryParse(isEnabledValue, out bool isEnabled))
            {
                demoSettings.IsEnabled = isEnabled;
            }

            if (int.TryParse(sessionDurationInMinutesValue, out int sessionDurationInMinutes))
            {
                demoSettings.SessionDurationInMinutes = sessionDurationInMinutes;
            }

            return demoSettings;
        }
    }
}
