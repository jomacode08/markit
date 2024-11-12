using markit.Application.Helpers;
using markit.Application.Models.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

                try
                {
                    await MarkitDbContextSeed.SeedAsync(context, userDefaultSettings);
                }
                catch (Exception)
                {
                    // TODO: Aplicar Logging, revisar rollback
                    throw;
                }
            }

            return webApplication;
        }
    }
}
