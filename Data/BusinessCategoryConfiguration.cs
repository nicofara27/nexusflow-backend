using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using NexusFlow.Models.Entities;

namespace NexusFlow.Data
{
    public class BusinessCategoryConfiguration : IEntityTypeConfiguration<BusinessCategory>
    {
        public void Configure(EntityTypeBuilder<BusinessCategory> builder)
        {
            builder.Property(bc => bc.Id)
                .ValueGeneratedOnAdd()
                .HasValueGenerator<SequentialGuidValueGenerator>();
            builder.Property(bc => bc.Name)
                .IsRequired()
                .HasMaxLength(100);
            builder.Property(bc => bc.Slug)
                .IsRequired()
                .HasMaxLength(100);
            builder.HasIndex(bc => bc.Slug)
                .IsUnique();
            builder.HasMany(bc => bc.Businesses)
                .WithOne(b => b.BusinessCategory)
                .HasForeignKey(b => b.BusinessCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasData(
                new BusinessCategory
                {
                    Id = Guid.Parse("123e4567-e89b-12d3-a456-426614174000"),
                    Name = "Barbería",
                    Slug = "barberia"
                },
                new BusinessCategory
                {
                    Id = Guid.Parse("6ec0bd7f-11c0-43da-975e-2a8ad9ebae0b"),
                    Name = "Peluquería",
                    Slug = "peluqueria"
                },
                new BusinessCategory
                {
                    Id = Guid.Parse("a1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d"),
                    Name = "Salón de uñas",
                    Slug = "salon-de-unas"
                },
                new BusinessCategory
                {
                    Id = Guid.Parse("f81d4fae-7dec-11d0-a765-00a0c91e6bf6"),
                    Name = "Estética",
                    Slug = "estetica"
                }
            );
        }
    }
}
