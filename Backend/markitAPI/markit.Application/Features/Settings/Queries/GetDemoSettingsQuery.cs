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
            string? userIdValue = GetSettingValue(SystemConfigKeys.DEMO_USER_ID_KEY, settings);
            string? tokenDurationInMinutesValue = GetSettingValue(SystemConfigKeys.DEMO_TOKEN_DURATION_IN_MINUTES_KEY, settings);
            
            return ConstructDemoSettings(
                isEnabledValue,
                userIdValue,
                tokenDurationInMinutesValue
            );
        }

        private static string? GetSettingValue(string key, IReadOnlyList<SystemConfig> settings)
        {
            return settings.FirstOrDefault(s => s.Id.Equals(key, StringComparison.OrdinalIgnoreCase))?.Value;
        }

        private static DemoSettings ConstructDemoSettings(
            string? isEnabledValue,
            string? userIdValue,
            string? tokenDurationInMinutesValue
        ) {
            DemoSettings demoSettings = new();

            if (bool.TryParse(isEnabledValue, out bool isEnabled))
            {
                demoSettings.IsEnabled = isEnabled;
            }

            if (int.TryParse(tokenDurationInMinutesValue, out int tokenDurationInMinutes))
            {
                demoSettings.TokenDurationInMinutes = tokenDurationInMinutes;
            }

            demoSettings.UserId = userIdValue;
            return demoSettings;
        }
    }
}
