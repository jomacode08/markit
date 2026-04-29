using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using static markit.Application.Helpers.GeneralConstant;

namespace markit.Infrastructure.Security.Configurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<IdentityRole>
    {
        public void Configure(EntityTypeBuilder<IdentityRole> builder)
        {
            CrearRolesDefault(builder);
        }

        private static void CrearRolesDefault(EntityTypeBuilder<IdentityRole> builder)
        {
            IdentityRole admin = ConstructIdentityRole(id: Role.ADMIN_UUID, name: Role.ADMIN_NAME);
            IdentityRole general = ConstructIdentityRole(id: Role.GENERAL_UUID, name: Role.GENERAL_NAME);
            IdentityRole demo = ConstructIdentityRole(id: Role.DEMO_UUID, name: Role.DEMO_NAME);

            builder.HasData(
                admin,
                general,
                demo
            );
        }

        private static IdentityRole ConstructIdentityRole(string id, string name)
        {
            return new IdentityRole()
            {
                Id = id,
                Name = name,
                NormalizedName = name.ToUpper(),
            };
        }
    }
}
