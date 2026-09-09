using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using NexusFlow.Models.Entities;

namespace NexusFlow.Data.Configuration
{
    public class BusinessConfiguration : IEntityTypeConfiguration<Business>
    {
        public void Configure(EntityTypeBuilder<Business> builder)
        {
            builder.Property(b => b.Id)
                .ValueGeneratedOnAdd()
                .HasValueGenerator<SequentialGuidValueGenerator>();
            builder.Property(b => b.Name)
                .IsRequired()
                .HasMaxLength(100);
            builder.Property(b => b.Address)
                .IsRequired()
                .HasMaxLength(100);
            builder.Property(x => x.About)
                .HasMaxLength(2000);
            builder.Property(x => x.Latitude)
                .HasPrecision(10, 7);
            builder.Property(x => x.Longitude)
                .HasPrecision(10, 7);
            builder.Property(x => x.AccentColor)
                .HasMaxLength(7);
        }
    }
}
