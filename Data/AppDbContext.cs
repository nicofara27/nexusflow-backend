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
        public DbSet<Business> Business => Set<Business>();
        public DbSet<UserBusiness> UsersBusiness => Set<UserBusiness>();
        public DbSet<Service> Services => Set<Service>();
        public DbSet<Appointment> Appointments => Set<Appointment>();
        public DbSet<EmployeeSchedule> EmployeeSchedules => Set<EmployeeSchedule>();
        public DbSet<ServiceAssignment> ServiceAssignment => Set<ServiceAssignment>();
        public DbSet<BusinessCategory> BusinessCategories => Set<BusinessCategory>();
        public DbSet<ServiceCategory> ServiceCategories => Set<ServiceCategory>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
