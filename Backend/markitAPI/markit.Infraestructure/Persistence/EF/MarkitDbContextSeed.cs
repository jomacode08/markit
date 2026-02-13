using markit.Application.Models.Authentication;
using markit.Application.Models.Authentication.AppUser;
using markit.Application.Models.Authentication.Enums;
using markit.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Transactions;
using static markit.Application.Helpers.GeneralConstant;

namespace markit.Infraestructure.Persistence.EF
{
    public static class MarkitDbContextSeed
    {
        public static async Task SeedAsync(
            MarkitDbContext context,
            UserManager<AppUser> userManager,
            UserDefaultSettings userDefaultSettings
        )
        {
            // Aplicate any pending migration or create database for the first time.
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

        private static async Task<int> CreateAdministrator(MarkitDbContext context, UserManager<AppUser> userManager, UserDefaultSettings userDefaultSettings)
        {
            using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);

            // Add creator
            Collection mainCollection = GetMainCollection();
            Creator creator = new()
            {
                FirstName = userDefaultSettings.FirstName,
                LastName = userDefaultSettings.LastName,
                Collections = [mainCollection]
            };
            context.Creators.Add(creator);
            await context.SaveChangesAsync();


            // Update main collection path
            mainCollection.Path = $"{mainCollection.Id}";
            context.Collections.Update(mainCollection);
            await context.SaveChangesAsync();

            // Add app user
            PasswordHasher<AppUser> hasher = new();
            AppUser adminUser = new()
            {
                Id = userDefaultSettings.Id,
                UserName = userDefaultSettings.UserName,
                GivenName = $"{userDefaultSettings.FirstName} {userDefaultSettings.LastName}",
                Email = userDefaultSettings.UserName,
                NormalizedUserName = userDefaultSettings.UserName.ToUpper(),
                NormalizedEmail = userDefaultSettings.UserName.ToUpper(),
                PhoneNumber = "0000000000",
                PhoneNumberConfirmed = true,
                EmailConfirmed = true,
                TwoFactorEnabled = false,
                LockoutEnabled = false,
                RegistrationConfirmed = true,
                AccessFailedCount = 0,
                AccessType = AccessType.Internal,
                CreatedDate = DateTime.UtcNow,
                CreatorId = creator.Id,
            };

            string passwordHashed = hasher.HashPassword(adminUser, userDefaultSettings.Password);
            adminUser.PasswordHash = passwordHashed;
            await userManager.CreateAsync(adminUser);
            await userManager.AddToRoleAsync(adminUser, Role.ADMIN_NAME);

            scope.Complete();
            return creator.Id;
        }

        private static Collection GetMainCollection() => new()
        {
            Name = Marks.MAIN_COLLECTION_NAME,
            PathNames = $"/{Marks.MAIN_COLLECTION_NAME}",
            IsMain = true
        };
    }
}
