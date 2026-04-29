using markit.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NpgsqlTypes;

namespace markit.Infrastructure.Persistence.EF.Configurations
{
    public class BlockConfiguration : IEntityTypeConfiguration<Block>
    {
        public void Configure(EntityTypeBuilder<Block> builder)
        {
            builder.Property<NpgsqlTsVector>("SearchVector")
                .IsGeneratedTsVectorColumn("english", "Title", "Content");

            builder.HasIndex("SearchVector")
                .HasMethod("GIN");
        }
    }
}
