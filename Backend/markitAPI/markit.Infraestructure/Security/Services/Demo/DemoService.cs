using markit.Application.Contracts.Authentication;
using markit.Application.Contracts.Authentication.Demo;
using markit.Application.Contracts.Settings;
using markit.Application.Exceptions;
using markit.Application.Models.Authentication.AppUser;
using markit.Application.Models.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using static markit.Application.Helpers.GeneralConstant;

namespace markit.Infraestructure.Security.Services.Demo
{
    public class DemoService : IDemoService
    {
        private readonly ILogger<DemoService> _logger;
        private readonly IJwtService _jwtService;
        private readonly ISettingsService _settingsService;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;

        private const string CONFIGURATION_ERROR_MESSAGE = "Demo mode is not available due to a configuration error.";
        private const string DEMO_MODE_DISABLED_ERROR_MESSAGE = "Demo mode is currently disabled.";
        private const string DEMO_USER_NOT_CONFIGURED_ERROR_MESSAGE = "The demo user has not been configured in the application.";

        private const string DEMO_LOGIN_ATTEMPT_MESSAGE = "Demo login attempt.";
        private const string SUCCESSFUL_DEMO_LOGIN_MESSAGE = "Successful demo login.";
        private const string DEMO_MODE_DISABLED_MESSAGE = "Demo mode was disabled due to user demo availability or a role configuration error.";

        public DemoService(
            ILogger<DemoService> logger,
            IJwtService jwtService,
            ISettingsService settingsService, 
            SignInManager<AppUser> signInManager,
            UserManager<AppUser> userManager
        )
        {
            _logger = logger;
            _jwtService = jwtService;
            _settingsService = settingsService;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public async Task<AppUser> LoginAsync(HttpContext context)
        {
            CreateInformationLogEntry(DEMO_LOGIN_ATTEMPT_MESSAGE);
            if (!await IsDemoEnabledAsync()) throw new CustomValidationException(DEMO_MODE_DISABLED_ERROR_MESSAGE);
            
            AppUser demoUser = await GetUserDemoAsync();
            await EnsureDemoUserIsValidAsync(demoUser);
            await AuthenticateDemoUserAsync(demoUser, context);
            return demoUser;
        }

        private async Task AuthenticateDemoUserAsync(AppUser user, HttpContext context)
        {
            await _jwtService.IssueDemoTokenAsync(user, context);
            await _signInManager.SignInAsync(user, isPersistent: false);
            CreateInformationLogEntry(SUCCESSFUL_DEMO_LOGIN_MESSAGE);
        }

        private async Task EnsureDemoUserIsValidAsync(AppUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains(Role.DEMO_NAME) || roles.Count != 1 || !user.Enabled)
            {
                await DisableDemoModeAsync();
                throw new CustomValidationException(CONFIGURATION_ERROR_MESSAGE);
            }
        }

        private async Task<bool> IsDemoEnabledAsync()
        {
            string? isDemoEnabledValue = await _settingsService.GetValueAsync(SystemConfigKeys.IS_DEMO_ENABLED_KEY);
            if (!bool.TryParse(isDemoEnabledValue, out bool isDemoEnabled)) return false;
            return isDemoEnabled;
        }

        private async Task<AppUser> GetUserDemoAsync()
        {
            string demoUserId = await _settingsService.GetValueAsync(SystemConfigKeys.DEMO_USER_ID_KEY)
                ?? throw new CustomValidationException(DEMO_USER_NOT_CONFIGURED_ERROR_MESSAGE);
            return await _userManager.FindByIdAsync(demoUserId)
                ?? throw new NotFoundException("Users", demoUserId);
        }

        private async Task DisableDemoModeAsync()
        {
            string key = SystemConfigKeys.IS_DEMO_ENABLED_KEY;
            SystemConfig isDemoEnabledSetting = await _settingsService.GetAsync(key)
                ?? throw new NotFoundException("SystemConfigs", key);
            isDemoEnabledSetting.Value = "false";
            await _settingsService.UpdateAsync(isDemoEnabledSetting);
            CreateInformationLogEntry(DEMO_MODE_DISABLED_MESSAGE);
        }

        private void CreateInformationLogEntry(string message)
        {
            _logger.LogInformation("{LogMessage}", message);
        }
    }
}
