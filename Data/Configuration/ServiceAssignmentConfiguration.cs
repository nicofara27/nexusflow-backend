using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using NexusFlow.Models.Entities;

namespace NexusFlow.Data.Configurations
{
    public class ServiceAssignmentConfiguration : IEntityTypeConfiguration<ServiceAssignment>
    {
        public void Configure(EntityTypeBuilder<ServiceAssignment> builder)
        {
            builder.Property(sa => sa.Id)
                .ValueGeneratedOnAdd()
                .HasValueGenerator<SequentialGuidValueGenerator>();
        }
    }
}