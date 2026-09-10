using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using NexusFlow.Models.Entities;

namespace NexusFlow.Data.Configurations
{
    public class BusinessScheduleConfiguration : IEntityTypeConfiguration<BusinessSchedule>
    {
        public void Configure(EntityTypeBuilder<BusinessSchedule> builder)
        {
            builder.Property(bs => bs.Id)
                .ValueGeneratedOnAdd()
                .HasValueGenerator<SequentialGuidValueGenerator>();

            builder.HasIndex(bs => new
            {
                bs.BusinessId,
                bs.DayOfWeek
            })
            .IsUnique();

            builder.HasOne(bs => bs.Business)
                .WithMany(b => b.Schedules)
                .HasForeignKey(bs => bs.BusinessId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}