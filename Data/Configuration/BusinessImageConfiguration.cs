using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace NexusFlow.Data.Configurations
{
    public class BusinessImageConfiguration : IEntityTypeConfiguration<BusinessImage>
    {
        public void Configure(EntityTypeBuilder<BusinessImage> builder)
        {
            builder.Property(bi => bi.Id)
                .ValueGeneratedOnAdd()
                .HasValueGenerator<SequentialGuidValueGenerator>();

            builder.Property(bi => bi.StorageKey)
                .IsRequired()
                .HasMaxLength(1000);

            builder.HasIndex(bi => new
            {
                bi.BusinessId,
                bi.Order
            });

            builder.HasOne(bi => bi.Business)
                .WithMany(b => b.Images)
                .HasForeignKey(bi => bi.BusinessId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}