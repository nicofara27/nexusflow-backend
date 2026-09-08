using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using NexusFlow.Models.Entities;

namespace NexusFlow.Data.Configurations
{
    public class UserBusinessConfiguration : IEntityTypeConfiguration<UserBusiness>
    {
        public void Configure(EntityTypeBuilder<UserBusiness> builder)
        {
            builder.Property(ub => ub.Id)
                .ValueGeneratedOnAdd()
                .HasValueGenerator<SequentialGuidValueGenerator>();

            builder.HasIndex(ub => new { ub.UserId, ub.BusinessId })
                .IsUnique();

            builder.Property(ub => ub.Role)
                .HasConversion<string>();
        }
    }
}