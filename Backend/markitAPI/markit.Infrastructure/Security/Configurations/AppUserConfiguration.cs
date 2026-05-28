using markit.Application.Models.Authentication.AppUser;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace markit.Infrastructure.Security.Configurations
{
    public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
    {
        public void Configure(EntityTypeBuilder<AppUser> builder)
        {
            builder.HasIndex(u => u.ExpiresAt)
                .HasFilter("expires_at IS NOT NULL");
        }
    }
}
