using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace markit.Infraestructure.Persistence.EF
{
    public static class MigrationManager
    {
        public async static Task<WebApplication> SeedDatabase(this WebApplication webApplication)
        {
            using (var scope = webApplication.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<MarkitDbContext>();

                try
                {
                    await MarkitDbContextSeed.SeedAsync(context);
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
