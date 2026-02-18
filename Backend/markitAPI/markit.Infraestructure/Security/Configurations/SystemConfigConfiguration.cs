using markit.Application.Models.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static markit.Application.Helpers.GeneralConstant;

namespace markit.Infraestructure.Security.Configurations
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
                value: "false",
                description: "Configuration that toggles the demo features of the app."
            );
            builder.HasData(isDemoEnabled);
        }

        private static SystemConfig ConstructSetting(string key, string value, string description)
        {
            return new SystemConfig()
            {
                Id = key,
                Value = value,
                Description = description
            };
        }
    }
}
