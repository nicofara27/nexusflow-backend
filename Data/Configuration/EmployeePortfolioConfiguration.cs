using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using NexusFlow.Models.Entities;

namespace NexusFlow.Data.Configuration
{
    public class EmployeePortfolioConfiguration : IEntityTypeConfiguration<EmployeePortfolioImage>
    {
        public void Configure(EntityTypeBuilder<EmployeePortfolioImage> builder)
        {
            builder.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasValueGenerator<SequentialGuidValueGenerator>();

            builder.Property(e => e.StorageKey)
                .IsRequired()
                .HasMaxLength(1000);

            builder.HasIndex(e => new
            {
                e.UserBusinessId,
                e.Order
            });

            builder.HasOne(e => e.UserBusiness)
                .WithMany(ub => ub.PortfolioImages)
                .HasForeignKey(ub => ub.UserBusinessId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
