using markit.Application.Models.Settings;

namespace markit.Application.Contracts.Settings
{
    public interface ISettingsService
    {
        Task<SystemConfig?> GetAsync(string key);
        Task<IReadOnlyList<SystemConfig>> GetForDemoAsync();
        Task<string?> GetValueAsync(string key);
        Task UpdateAsync(SystemConfig config);
    }
}
