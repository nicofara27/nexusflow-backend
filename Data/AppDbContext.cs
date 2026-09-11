using Microsoft.EntityFrameworkCore;
using NexusFlow.Models.Entities;

namespace NexusFlow.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Appointment> Appointments => Set<Appointment>();
        public DbSet<Business> Business => Set<Business>();
        public DbSet<BusinessCategory> BusinessCategories => Set<BusinessCategory>();
        public DbSet<BusinessImage> BusinessImages => Set<BusinessImage>();
        public DbSet<BusinessSchedule> BusinessSchedules => Set<BusinessSchedule>();
        public DbSet<EmployeeSchedule> EmployeeSchedules => Set<EmployeeSchedule>();
        public DbSet<Service> Services => Set<Service>();
        public DbSet<ServiceAssignment> ServiceAssignment => Set<ServiceAssignment>();
        public DbSet<ServiceCategory> ServiceCategories => Set<ServiceCategory>();
        public DbSet<User> Users => Set<User>();
        public DbSet<UserBusiness> UsersBusiness => Set<UserBusiness>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
