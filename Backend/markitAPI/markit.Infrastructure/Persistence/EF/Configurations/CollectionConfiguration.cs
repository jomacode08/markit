using markit.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace markit.infrastructure.Persistence.EF.Configurations
{
    public class CollectionConfiguration : IEntityTypeConfiguration<Collection>
    {
        public void Configure(EntityTypeBuilder<Collection> builder) {
            builder
                .HasOne(c => c.Parent)
                .WithMany(c => c.SubCollections)
                .HasForeignKey(c => c.ParentId)
                .OnDelete(DeleteBehavior.NoAction);

            builder
                .HasMany(c => c.Marks)
                .WithOne(m => m.Collection)
                .IsRequired()
                .HasForeignKey(m => m.CollectionId);

            builder.Property(c => c.Emoji)
                .IsUnicode();
        }
    }
}
