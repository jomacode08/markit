using markit.Application.Contracts.Settings;
using markit.Application.Exceptions;
using markit.Application.Models.Settings;
using markit.infrastructure.Persistence.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using static markit.Application.Helpers.GeneralConstant;

namespace markit.infrastructure.Security.Services.Settings
{
    public class SettingsService : ISettingsService
    {
        private readonly MarkitDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _defaultCacheExpiration = TimeSpan.FromHours(1);
        private const string DEMO_SETTINGS_CACHE_KEY = "demo_settings";
        private const string DEMO_KEY_PREFIX = "Demo";

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

        public async Task<IReadOnlyList<SystemConfig>> GetForDemoAsync()
        {
            if (_cache.TryGetValue(DEMO_SETTINGS_CACHE_KEY, out IReadOnlyList<SystemConfig>? cached) && cached != null)
                return cached;

            var result = await _context.SystemConfigs
                .AsNoTracking()
                .Where(s =>
                    s.Id.Equals(SystemConfigKeys.IS_DEMO_ENABLED_KEY)
                    || s.Id.Equals(SystemConfigKeys.DEMO_USER_ID_KEY)
                    || s.Id.Equals(SystemConfigKeys.DEMO_TOKEN_DURATION_IN_MINUTES_KEY)
                ).ToListAsync();

            _cache.Set(
                key: DEMO_SETTINGS_CACHE_KEY,
                value: result.AsReadOnly(),
                absoluteExpirationRelativeToNow: _defaultCacheExpiration
            );
            return result;
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
                        absoluteExpirationRelativeToNow: _defaultCacheExpiration
                    );
                }
            }

            return value;
        }

        public async Task AddAsync(SystemConfig config)
        {
            _context.Add(config);
            await _context.SaveChangesAsync();
            InvalidateGroupCache(GetKeyPrefix(config.Id));
        }

        public async Task AddRangeAsync(IEnumerable<SystemConfig> configs)
        {
            if (configs == null || !configs.Any()) return;
            _context.AddRange(configs);
            await _context.SaveChangesAsync();
            foreach (string prefix in configs.Select(c => GetKeyPrefix(c.Id)).Distinct())
            {
                InvalidateGroupCache(GetKeyPrefix(prefix));
            }
        }

        public async Task UpdateAsync(string key, string value)
        {
            SystemConfig existing = await GetAsync(key)
                ?? throw new NotFoundException("SystemConfigs", key);

            existing.Value = value;
            await _context.SaveChangesAsync();

            // Invalidate the cache so the next request pulls the fresh DB data.
            InvalidateCache(key: key);
            InvalidateGroupCache(GetKeyPrefix(key));
        }

        public async Task UpdateRangeAsync(IEnumerable<SystemConfig> configs)
        {
            if (configs == null || !configs.Any()) return;
            _context.UpdateRange(configs);
            await _context.SaveChangesAsync();

            foreach (SystemConfig config in configs)
            {
                InvalidateCache(config.Id);
            }

            foreach (string prefix in configs.Select(c => GetKeyPrefix(c.Id)).Distinct())
            {
                InvalidateGroupCache(GetKeyPrefix(prefix));
            }
        }

        private void InvalidateCache(string key)
        {
            _cache.Remove(key);
        }

        private void InvalidateGroupCache(string prefix)
        {
            string? groupKey = GetGroupCacheKey(prefix);
            if (groupKey != null) InvalidateCache(groupKey);
        }

        private static string GetKeyPrefix(string key)
        {
            int colonIndex = key.IndexOf(':');
            return colonIndex != -1 ? key[..colonIndex] : key;
        }

        private static string? GetGroupCacheKey(string prefix)
        {
            return prefix switch
            {
                DEMO_KEY_PREFIX => DEMO_SETTINGS_CACHE_KEY,
                _ => null
            };
        }
    }
}
