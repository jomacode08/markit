using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using markit.Infraestructure.Security.Models;
using markit.Application.Models.Authentication;
using markit.Application.Helpers;

namespace markit.Infraestructure.Security.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        private readonly UserDefaultSettings _userDefaultSettings;

        public UserConfiguration(UserDefaultSettings userDefaultSettings)
        {
            _userDefaultSettings = userDefaultSettings;
        }

        public void Configure(EntityTypeBuilder<User> builder)
        {
            CreateAdmin(builder);
        }

        private void CreateAdmin(EntityTypeBuilder<User> builder)
        {
            PasswordHasher<User> hasher = new();

            User superUsuario = new()
            {
                Id = _userDefaultSettings.Id,
                FirstName = _userDefaultSettings.FirstName,
                LastName = _userDefaultSettings.LastName,
                UserName = _userDefaultSettings.UserName,
                NormalizedUserName = _userDefaultSettings.UserName,
                Email = _userDefaultSettings.UserName,
                NormalizedEmail = _userDefaultSettings.UserName,
                PhoneNumber = "0000000000",
                PhoneNumberConfirmed = true,
                EmailConfirmed = true,
                TwoFactorEnabled = false,
                LockoutEnabled = false,
                AccessFailedCount = 0,
                CreatedDate = DateTime.UtcNow,
            };

            string passwordHashed = hasher.HashPassword(superUsuario, _userDefaultSettings.Password);
            superUsuario.PasswordHash = passwordHashed;

            builder.HasData(superUsuario);
        }
    }
}
