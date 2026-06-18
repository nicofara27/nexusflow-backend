using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using NexusFlow.Models.Entities;

namespace NexusFlow.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Business> Business => Set<Business>();
        public DbSet<UserBusiness> UsersBusiness => Set<UserBusiness>();
        public DbSet<Service> Services => Set<Service>();
        public DbSet<Appointment> Appointments => Set<Appointment>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .Property(u => u.Id)
                .ValueGeneratedOnAdd()
                .HasValueGenerator<SequentialGuidValueGenerator>();

            modelBuilder.Entity<UserBusiness>(entity =>
            {
                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd()
                    .HasValueGenerator<SequentialGuidValueGenerator>();

                entity.HasIndex(ub => new { ub.UserId, ub.BusinessId })
                    .IsUnique();
                entity.Property(e => e.Role)
                    .HasConversion<string>();
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
