using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using static markit.Application.Helpers.GeneralConstant;

namespace markit.Infraestructure.Security.Configurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<IdentityRole>
    {
        public void Configure(EntityTypeBuilder<IdentityRole> builder)
        {
            CrearRolesDefault(builder);
        }

        private static void CrearRolesDefault(EntityTypeBuilder<IdentityRole> builder)
        {
            IdentityRole admin = new()
            {
                Id = Role.adminUuid,
                Name = Role.admin,
                NormalizedName = Role.admin.ToLower(),
            };

            IdentityRole general = new()
            {
                Id = Role.generalUuid,
                Name = Role.general,
                NormalizedName = Role.general.ToLower(),
            };

            builder.HasData(
                admin,
                general
            );
        }
    }
}
