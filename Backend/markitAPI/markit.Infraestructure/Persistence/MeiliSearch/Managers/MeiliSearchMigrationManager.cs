using markit.Application.Helpers;
using markit.Application.Models.Authentication.MeiliSearch;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace markit.Infraestructure.Persistence.MeiliSearch.Managers
{
    public static class MeiliSearchMigrationManager
    {
        public async static Task<WebApplication> SeedIndexes(this WebApplication app, IConfiguration configuration)
        {
            var settings = new MeiliSearchAuthSettings();
            configuration.Bind(GeneralConstant.Configuration.meiliSearchSettingsName, settings);

            using (var scope = app.Services.CreateScope())
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<object>>();
                try
                {
                    await MeiliSearchSeed.SeedAsync(settings);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"An error ocurred while seeding the meiliSearch instance: {ex.Message}");
                    throw;
                }
            }

            return app;
        }
    }
}
