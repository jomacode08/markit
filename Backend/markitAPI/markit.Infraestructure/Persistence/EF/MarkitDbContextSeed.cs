using markit.Application.Exceptions;
using markit.Application.Models.Authentication;
using markit.Domain.Entities;
using markit.Infraestructure.Security.Models;
using Microsoft.EntityFrameworkCore;
using System.Transactions;

namespace markit.Infraestructure.Persistence.EF
{
    public static class MarkitDbContextSeed
    {
        public static async Task SeedAsync(MarkitDbContext context, UserDefaultSettings userDefaultSettings)
        {
            // Aplicate any pending migration or create database for the first time.
            await context.Database.MigrateAsync();

            // Validate if the database is active
            bool databaseIsActive = await context.Database.CanConnectAsync();
            if (!databaseIsActive) throw new Exception("It hasn't been posible to connect to the database");
           
            if (!context.Creators.Any())
            {
                await AddAdminCreator(context, userDefaultSettings);
            }
        }

        private static async Task AddAdminCreator(MarkitDbContext context, UserDefaultSettings userDefaultSettings)
        {
            using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);

            // Create Admin Creator
            Creator creator = new()
            {
                FirstName = userDefaultSettings.FirstName,
                LastName = userDefaultSettings.LastName,
            };

            context.Creators.Add(creator);
            await context.SaveChangesAsync();

            // Update CreatorId from Admin AppUser
            AppUser user = context.User.Find(userDefaultSettings.Id) 
            ?? throw new CustomValidationException("The admin user hasn't been configured");

            user.CreatorId = creator.Id;
            context.Creators.Update(creator);
            await context.SaveChangesAsync();

            scope.Complete();
        }
    }
}
