using markit.Application.Contracts.Settings;
using markit.Application.Exceptions;
using markit.Application.Models.Settings;
using markit.Infraestructure.Persistence.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace markit.Infraestructure.Security.Services.Settings
{
    public class SettingsService : ISettingsService
    {
        private readonly MarkitDbContext _context;
        private readonly IMemoryCache _cache;
        private const int CACHE_EXPIRE_IN_HOURS = 1;

        public SettingsService(MarkitDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<SystemConfig?> GetAsync(string key)
        {
            return await _context.SystemConfigs
                .FirstOrDefaultAsync(s => s.Id.Equals(key));
        }

        public async Task<string?> GetValueAsync(string key)
        {
            // Try to get the value from cache
            if (!_cache.TryGetValue(key, out string? value))
            {
                // If not in cache, get from DB
                SystemConfig? setting = await GetAsync(key);
                // Set setting value to cache
                if (setting != null)
                {
                    value = setting.Value;
                    _cache.Set(
                        key,
                        value,
                        absoluteExpirationRelativeToNow: TimeSpan.FromHours(CACHE_EXPIRE_IN_HOURS)
                    );
                }
            }

            return value;
        }

        public async Task UpdateAsync(SystemConfig config)
        {
            SystemConfig existing = await GetAsync(config.Id)
                ?? throw new NotFoundException("SystemConfigs", config.Id);

            existing.Value = config.Value;
            existing.Description = config.Description;

            _context.Update(existing);
            // Invalidate the cache so the next request pulls the fresh DB data.
            _cache.Remove(config.Id);
            await _context.SaveChangesAsync();
        }
    }
}
