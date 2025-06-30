using markit.Application.Helpers;
using markit.Application.Models.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace markit.Infraestructure.Persistence.EF
{
    public static class MigrationManager
    {
        private static readonly string userDefaultSectionName = GeneralConstant.Configuration.userDefaultSectionName;
        public async static Task<WebApplication> SeedDatabase(this WebApplication webApplication,
            IConfiguration configuration)
        {
            // Bind the userDefaultSetting to the configuration section of the appSettings.json
            var userDefaultSettings = new UserDefaultSettings();
            configuration.Bind(userDefaultSectionName, userDefaultSettings);

            using (var scope = webApplication.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<MarkitDbContext>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<object>>();

                try
                {
                    await MarkitDbContextSeed.SeedAsync(context, userDefaultSettings);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error ocurred while seeding the database: {ex.Message}" , ex.Message);
                    throw;
                }
            }

            return webApplication;
        }
    }
}
