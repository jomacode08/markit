using markit.Application.Models.Authentication;
using markit.Application.Models.Authentication.AppUser;
using markit.Application.Models.Authentication.Enums;
using markit.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Transactions;
using static markit.Application.Helpers.GeneralConstant;

namespace markit.Infrastructure.Persistence.EF
{
    public static class MarkitDbContextSeed
    {
        public static async Task SeedAsync(
            MarkitDbContext context,
            UserManager<AppUser> userManager,
            UserDefaultSettings userDefaultSettings
        )
        {
            // Applicate any pending migration or create database for the first time.
            await context.Database.MigrateAsync();
            // Validate if the database is active
            bool databaseIsActive = await context.Database.CanConnectAsync();
            if (!databaseIsActive) throw new Exception("It hasn't been posible to connect to the database");
            // Create administrator
            if (!await IsAdminConfigurated(context, userManager))
            {
                await CreateAdministrator(context, userManager, userDefaultSettings);
            }
        }

        private static async Task<bool> IsAdminConfigurated(MarkitDbContext context, UserManager<AppUser> userManager)
        {
            var admins = await userManager.GetUsersInRoleAsync(Role.ADMIN_NAME);
            return admins.Any();
        }

        private static async Task<string> CreateAdministrator(MarkitDbContext context, UserManager<AppUser> userManager, UserDefaultSettings userDefaultSettings)
        {
            Collection mainCollection = ConstructMainCollection();
            using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);

            // Create administrator user
            PasswordHasher<AppUser> hasher = new();
            AppUser admin = ConstructAdministrator(userDefaultSettings, mainCollection);
            string passwordHashed = hasher.HashPassword(admin, userDefaultSettings.Password);
            admin.PasswordHash = passwordHashed;
            await userManager.CreateAsync(admin);
            await userManager.AddToRoleAsync(admin, Role.ADMIN_NAME);

            // Update main collection path
            mainCollection.Path = $"{mainCollection.Id}";
            context.Collections.Update(mainCollection);
            await context.SaveChangesAsync();

            scope.Complete();
            return admin.Id;
        }

        private static Collection ConstructMainCollection() => new()
        {
            Name = Notebooks.MAIN_COLLECTION_NAME,
            PathNames = $"/{Notebooks.MAIN_COLLECTION_NAME}",
            IsMain = true
        };

        private static AppUser ConstructAdministrator(UserDefaultSettings settings, Collection mainCollection) => new()
        {
            UserName = settings.UserName,
            Email = settings.UserName,
            NormalizedUserName = settings.UserName.ToUpper(),
            NormalizedEmail = settings.UserName.ToUpper(),
            GivenName = "Markit admin",
            PhoneNumber = "0000000000",
            PhoneNumberConfirmed = true,
            EmailConfirmed = true,
            TwoFactorEnabled = false,
            LockoutEnabled = false,
            Enabled = true,
            AccessFailedCount = 0,
            AccessType = AccessType.Internal,
            CreatedDate = DateTime.UtcNow,
            Collections = [mainCollection]
        };
    }
}
