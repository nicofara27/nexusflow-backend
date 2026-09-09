using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using NexusFlow.Models.Entities;

namespace NexusFlow.Data.Configuration
{
    public class ServiceCategoryConfiguration : IEntityTypeConfiguration<ServiceCategory>
    {
        public void Configure(EntityTypeBuilder<ServiceCategory> builder) 
        {
            builder.Property(sc => sc.Id)
                .ValueGeneratedOnAdd()
                .HasValueGenerator<SequentialGuidValueGenerator>();
            builder.Property(sc => sc.Name)
                .IsRequired()
                .HasMaxLength(100);
            builder.HasIndex(sc => new { sc.BusinessId, sc.Name })
                 .IsUnique();
            builder.HasIndex(sc => new { sc.BusinessId, sc.Order });
            builder.HasOne(sc => sc.Business)
                .WithMany(b => b.ServiceCategories)
                .HasForeignKey(sc => sc.BusinessId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(sc => sc.Services)
                .WithOne(s => s.ServiceCategory)
                .HasForeignKey(s => s.ServiceCategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
