using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using markit.Infraestructure.Security.Models;
using markit.Application.Models.Authentication;
using markit.Application.Models.Authentication.Enums;

namespace markit.Infraestructure.Security.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<AppUser>
    {
        private readonly UserDefaultSettings _userDefaultSettings;

        public UserConfiguration(UserDefaultSettings userDefaultSettings)
        {
            _userDefaultSettings = userDefaultSettings;
        }

        public void Configure(EntityTypeBuilder<AppUser> builder)
        {
            CreateAdmin(builder);
        }

        private void CreateAdmin(EntityTypeBuilder<AppUser> builder)
        {
            PasswordHasher<AppUser> hasher = new();

            AppUser superUsuario = new()
            {
                Id = _userDefaultSettings.Id,
                UserName = _userDefaultSettings.UserName,
                GivenName = $"{ _userDefaultSettings.FirstName } { _userDefaultSettings.LastName }",
                Email = _userDefaultSettings.UserName,
                NormalizedUserName = _userDefaultSettings.UserName.ToUpper(),
                NormalizedEmail = _userDefaultSettings.UserName.ToUpper(),
                PhoneNumber = "0000000000",
                PhoneNumberConfirmed = true,
                EmailConfirmed = true,
                TwoFactorEnabled = false,
                LockoutEnabled = false,
                AccessFailedCount = 0,
                AccessType = AccessType.Internal,
                CreatedDate = DateTime.UtcNow,
            };

            string passwordHashed = hasher.HashPassword(superUsuario, _userDefaultSettings.Password);
            superUsuario.PasswordHash = passwordHashed;

            builder.HasData(superUsuario);
        }
    }
}
