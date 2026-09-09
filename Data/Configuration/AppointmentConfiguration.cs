using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using NexusFlow.Models.Entities;

namespace NexusFlow.Data.Configurations
{
    public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
    {
        public void Configure(EntityTypeBuilder<Appointment> builder)
        {
            builder.Property(a => a.Id)
                .ValueGeneratedOnAdd()
                .HasValueGenerator<SequentialGuidValueGenerator>();

            builder.Property(a => a.Status)
                .HasConversion<string>()
                .HasMaxLength(30);

            builder.HasIndex(a => new
            {
                a.EmployeeId,
                a.StartDate
            });

            builder.HasIndex(a => new
            {
                a.BusinessId,
                a.StartDate
            });
        }
    }
}