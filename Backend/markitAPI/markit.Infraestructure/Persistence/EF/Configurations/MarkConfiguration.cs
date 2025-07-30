using markit.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace markit.Infraestructure.Persistence.EF.Configurations
{
    public class MarkConfiguration : IEntityTypeConfiguration<Mark>
    {
        public void Configure(EntityTypeBuilder<Mark> builder)
        {
            builder.Property(c => c.Emoji)
                .IsUnicode()
                .UseCollation("Latin1_General_100_CI_AS_SC");
        }
    }
}
