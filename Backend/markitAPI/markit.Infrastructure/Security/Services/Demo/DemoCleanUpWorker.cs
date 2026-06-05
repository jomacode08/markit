using markit.Application.Contracts.Authentication.Demo;
using markit.Application.Contracts.Settings;
using markit.Application.Helpers;
using markit.Application.Models.Authentication.AppUser;
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

                    List<string> guestIds = await context.UserRoles
                        .AsNoTracking()
                        .Where(ur => ur.RoleId == GeneralConstant.Role.GUEST_UUID)
                        .Select(ur => ur.UserId)
                        .ToListAsync(cancellationToken);

                    List<AppUser> expiredGuests = await context.Users.AsNoTracking()
                        .Where(u => guestIds.Contains(u.Id) && u.ExpiresAt <= now)
                        .ToListAsync(cancellationToken);

                    context.Users.RemoveRange(expiredGuests);
                    await context.SaveChangesAsync(cancellationToken);                    
                    _logger.LogInformation("Successfully deleted {count} expired guest users.", expiredGuests.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to clean up expired guest users.");
            }
        }
    }
}
