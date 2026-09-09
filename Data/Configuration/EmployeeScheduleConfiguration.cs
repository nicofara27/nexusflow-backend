using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using NexusFlow.Models.Entities;

namespace NexusFlow.Data.Configurations
{
    public class EmployeeScheduleConfiguration
        : IEntityTypeConfiguration<EmployeeSchedule>
    {
        public void Configure(EntityTypeBuilder<EmployeeSchedule> builder)
        {
            builder.Property(es => es.Id)
                .ValueGeneratedOnAdd()
                .HasValueGenerator<SequentialGuidValueGenerator>();

            builder.HasIndex(es => new
            {
                es.UserBusinessId,
                es.DayOfWeek
            })
            .IsUnique();
        }
    }
}