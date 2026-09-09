using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using NexusFlow.Models.Entities;

namespace NexusFlow.Data.Configurations
{
    public class ServiceConfiguration : IEntityTypeConfiguration<Service>
    {
        public void Configure(EntityTypeBuilder<Service> builder)
        {
            builder.Property(s => s.Id)
                .ValueGeneratedOnAdd()
                .HasValueGenerator<SequentialGuidValueGenerator>();
            builder.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(150);
            builder.Property(s => s.Description)
                .HasMaxLength(1000);
            builder.HasIndex(s => s.BusinessId);
        }
    }
}