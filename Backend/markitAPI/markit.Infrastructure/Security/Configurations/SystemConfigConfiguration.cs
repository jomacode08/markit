using markit.Application.Common.Helpers;
using markit.Application.Models.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static markit.Application.Helpers.GeneralConstant;

namespace markit.Infrastructure.Security.Configurations
{
    public class SystemConfigConfiguration : IEntityTypeConfiguration<SystemConfig>
    {
        public void Configure(EntityTypeBuilder<SystemConfig> builder)
        {
            PopulateDefaultSettings(builder);
        }

        private static void PopulateDefaultSettings(EntityTypeBuilder<SystemConfig> builder)
        {
            SystemConfig isDemoEnabled = ConstructSetting(
                key: SystemConfigKeys.IS_DEMO_ENABLED_KEY,
                value: "false"
            );
            builder.HasData(isDemoEnabled);
        }

        private static SystemConfig ConstructSetting(string key, string value)
        {
            string description = Utilities.GetSystemConfigDescription(key)
                ?? throw new InvalidOperationException($"No description found for system config key: {key}");
            return new SystemConfig()
            {
                Id = key,
                Value = value,
                Description = description
            };
        }
    }
}
