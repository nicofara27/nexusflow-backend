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
    }
}
