using Microsoft.EntityFrameworkCore;
using NexusFlow.Data;
using NexusFlow.Models.Entities;
using NexusFlow.Repositories.Interfaces;

namespace NexusFlow.Repositories
{
    public class ServiceCategoryRepository : IServiceCategoryRepository
    {
        private readonly AppDbContext _context;
            
        public ServiceCategoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ServiceCategory>> GetByBusinessIdAsync(Guid businessId)
        {
            return await _context.ServiceCategories
                .Where(sc => sc.BusinessId == businessId)
                .OrderBy(sc => sc.Order)
                .ThenBy(sc => sc.Name)
                .ToListAsync();
        }

        public async Task<ServiceCategory?> GetByIdAsync(Guid id)
        {
            return await _context.ServiceCategories.FirstOrDefaultAsync(sc => sc.Id == id);
        } 

        public async Task<bool> ExistsByNameAsync(Guid businessId, string name, Guid? excludeId = null)
        {
            return await _context.ServiceCategories.AnyAsync(sc => 
                sc.BusinessId == businessId && 
                sc.Name == name && 
                (!excludeId.HasValue || sc.Id != excludeId.Value));
        }

        public void Add(ServiceCategory serviceCategory)
        {
            _context.ServiceCategories.Add(serviceCategory);
        }

        public void Delete(ServiceCategory serviceCategory)
        {
            _context.ServiceCategories.Remove(serviceCategory);
        }
    }
}
