using markit.Application.Models.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static markit.Application.Helpers.GeneralConstant;

namespace markit.Infraestructure.Security.Configurations
{
    public class UserRoleConfiguration : IEntityTypeConfiguration<IdentityUserRole<string>>
    {
        private readonly UserDefaultSettings _userDefaultSettings;

        public UserRoleConfiguration(UserDefaultSettings userDefaultSettings)
        {
            _userDefaultSettings = userDefaultSettings;
        }

        public void Configure(EntityTypeBuilder<IdentityUserRole<string>> builder)
        {
            SetRoleToAdminUser(builder);
        }

        private void SetRoleToAdminUser(EntityTypeBuilder<IdentityUserRole<string>> builder)
        {
            IdentityUserRole<string> userAdminRole = new() {
                RoleId = Role.adminUuid,
                UserId = _userDefaultSettings.Id
            };

            builder.HasData(userAdminRole);
        }
    }
}
