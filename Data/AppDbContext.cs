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
        public DbSet<EmployeeSchedule> EmployeeSchedules => Set<EmployeeSchedule>();
        public DbSet<ServiceAssignment> ServiceAssignment => Set<ServiceAssignment>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .Property(u => u.Id)
                .ValueGeneratedOnAdd()
                .HasValueGenerator<SequentialGuidValueGenerator>();

            modelBuilder.Entity<Business>()
                .Property(b => b.Id)
                .ValueGeneratedOnAdd()
                .HasValueGenerator<SequentialGuidValueGenerator>();

            modelBuilder.Entity<UserBusiness>(entity =>
            {
                entity.Property(ub => ub.Id)
                    .ValueGeneratedOnAdd()
                    .HasValueGenerator<SequentialGuidValueGenerator>();

                entity.HasIndex(ub => new { ub.UserId, ub.BusinessId })
                    .IsUnique();
                entity.Property(e => e.Role)
                    .HasConversion<string>();
            });

            modelBuilder.Entity<Service>()
                .Property(s => s.Id)
                .ValueGeneratedOnAdd()
                .HasValueGenerator<SequentialGuidValueGenerator>();

            modelBuilder.Entity<Appointment>()
                .Property(a => a.Id)
                .ValueGeneratedOnAdd()
                .HasValueGenerator<SequentialGuidValueGenerator>();

            modelBuilder.Entity<EmployeeSchedule>()
                .Property(a => a.Id)
                .ValueGeneratedOnAdd()
                .HasValueGenerator<SequentialGuidValueGenerator>();

            modelBuilder.Entity<ServiceAssignment>()
                .Property(a => a.Id)
                .ValueGeneratedOnAdd()
                .HasValueGenerator<SequentialGuidValueGenerator>();

            base.OnModelCreating(modelBuilder);
        }
    }
}
