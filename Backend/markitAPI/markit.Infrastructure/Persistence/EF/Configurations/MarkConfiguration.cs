using markit.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NpgsqlTypes;

namespace markit.Infrastructure.Persistence.EF.Configurations
{
    public class MarkConfiguration : IEntityTypeConfiguration<Notebook>
    {
        public void Configure(EntityTypeBuilder<Notebook> builder)
        {
            builder.Property(c => c.Emoji)
                .IsUnicode();

            builder.Property<NpgsqlTsVector>("SearchVector")
                .IsGeneratedTsVectorColumn("english", "Name");

            builder.HasIndex("SearchVector")
                .HasMethod("GIN");
        }
    }
}
