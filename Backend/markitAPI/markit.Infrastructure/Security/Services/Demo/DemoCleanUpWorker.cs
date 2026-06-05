using markit.Application.Contracts.Authentication.Demo;
using markit.Application.Contracts.Settings;
using markit.Application.Helpers;
using markit.Infrastructure.Persistence.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace markit.Infrastructure.Security.Services.Demo
{
    public class DemoCleanUpWorker : BackgroundService, IDemoCleanUpWorker
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<DemoCleanUpWorker> _logger;

        private readonly SemaphoreSlim _signal = new(0, 1);
        private readonly TimeSpan _enabledRunPeriod = TimeSpan.FromHours(1);

        public DemoCleanUpWorker(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<DemoCleanUpWorker> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (await IsDemoAvailable())
                {
                    _logger.LogInformation("Worker active: Executing database cleanup...");
                    await CleanUpExpiredUsers(stoppingToken);
                    // Wait for a defined time OR until a new signal arrives (whichever comes first)
                    _logger.LogInformation($"Worker resting. Will run again in {{totalMinutes}} minutes.", _enabledRunPeriod.TotalMinutes);
                    await _signal.WaitAsync(_enabledRunPeriod, stoppingToken);
                }
                else
                {
                    _logger.LogInformation("Worker entering deep sleep mode. Awaiting activation signal...");
                    // Sleep indefinitely (Timeout.InfiniteTimeSpan) until _signal.Release() is called
                    await _signal.WaitAsync(Timeout.InfiniteTimeSpan, stoppingToken);
                }
            }
        }

        public void Trigger()
        {
            _logger.LogInformation("Demo cleanup requested.");
            if (_signal.CurrentCount == 0)
            {
                _signal.Release();
            }
        }

        private async Task<bool> IsDemoAvailable()
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                ISettingsService settingsService = scope.ServiceProvider.GetRequiredService<ISettingsService>();
                string? isDemoEnabledValue = await settingsService.GetValueAsync(GeneralConstant.SystemConfigKeys.IS_DEMO_ENABLED_KEY);
                if (!bool.TryParse(isDemoEnabledValue, out bool isDemoEnabled)) return false;
                return isDemoEnabled;
            }
        }

        private async Task CleanUpExpiredUsers(CancellationToken cancellationToken)
        {
            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    MarkitDbContext context = scope.ServiceProvider.GetRequiredService<MarkitDbContext>();
                    DateTime now = DateTime.UtcNow;
                    int deletedCount = await context.Users
                                .Where(u => context.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == GeneralConstant.Role.GUEST_UUID)
                                            && u.ExpiresAt <= now)
                                .ExecuteDeleteAsync(cancellationToken);
                    _logger.LogInformation("Successfully deleted {count} expired guest users.", deletedCount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to clean up expired guest users.");
            }
        }
    }
}
