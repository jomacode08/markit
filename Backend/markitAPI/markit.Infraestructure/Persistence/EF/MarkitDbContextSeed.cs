using Microsoft.EntityFrameworkCore;

namespace markit.Infraestructure.Persistence.EF
{
    public static class MarkitDbContextSeed
    {
        public static async Task SeedAsync(MarkitDbContext context)
        {
            // Aplicate any pending migration or create database for the first time.
            await context.Database.MigrateAsync();

            // Validate if the database is active
            // bool databaseIsActive = await context.Database.CanConnectAsync();
            // if (!databaseIsActive) throw new Exception("It hasn't been posible to connect to the database");
            
            // Any initial information seed must to be here... 
        }
    }
}
