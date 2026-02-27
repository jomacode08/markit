using markit.Application.Models.Settings;

namespace markit.Application.Contracts.Settings
{
    public interface ISettingsService
    {
        Task<SystemConfig?> GetAsync(string key);
        Task<IReadOnlyList<SystemConfig>> GetForDemoAsync();
        Task<string?> GetValueAsync(string key);
        Task AddAsync(SystemConfig config);
        Task AddRangeAsync(IEnumerable<SystemConfig> configs);
        Task UpdateAsync(string key, string value);
        Task UpdateRangeAsync(IEnumerable<SystemConfig> configs);
    }
}
