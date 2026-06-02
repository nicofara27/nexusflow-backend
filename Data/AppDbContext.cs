using Microsoft.EntityFrameworkCore;
using NexusFlow.Models.Entities;

namespace NexusFlow.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Business> Businesses => Set<Business>();
        public DbSet<UserBusiness> UsersBusiness => Set<UserBusiness>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserBusiness>(entity =>
            {
                entity.Property(e => e.Id)
                    .HasValueGenerator<Microsoft.EntityFrameworkCore.ValueGeneration.SequentialGuidValueGenerator>();

                entity.HasIndex(ub => new { ub.UserId, ub.BusinessId })
                    .IsUnique();
                entity.Property(e => e.Role)
                    .HasConversion<string>();
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
