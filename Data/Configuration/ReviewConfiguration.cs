using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using NexusFlow.Models;

namespace NexusFlow.Data.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.Property(r => r.Id)
            .ValueGeneratedOnAdd()
            .HasValueGenerator<SequentialGuidValueGenerator>();

        builder.Property(r => r.Rating)
            .IsRequired();

        builder.Property(r => r.Comment)
            .HasMaxLength(1000);

        builder.HasIndex(r => r.AppointmentId)
            .IsUnique();

        builder.HasIndex(r => new
        {
            r.BusinessId,
            r.CreatedAt
        });

        builder.HasOne(r => r.Appointment)
            .WithOne(a => a.Review)
            .HasForeignKey<Review>(r => r.AppointmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Business)
            .WithMany(b => b.Reviews)
            .HasForeignKey(r => r.BusinessId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}