using Microsoft.EntityFrameworkCore;
using NexusFlow.Data;
using NexusFlow.Models.Entities;
using NexusFlow.Repositories.Interfaces;

namespace NexusFlow.Repositories.Implementations
{
    public class ServiceRepository : IServiceRepository
    {
        private readonly AppDbContext _context;

        public ServiceRepository(AppDbContext context)
        {
            _context = context;
        }

        public void Add(Service service)
        {
            _context.Services.Add(service);
        }
       
        public async Task<Service?> GetByServiceIdAsync(Guid serviceId)
        {
            return await _context.Services.FindAsync(serviceId);
        }
        
        public async Task<List<Service>> GetByIdsAsync(List<Guid> serviceIds)
        {
            return await _context.Services
                .Where(s => serviceIds.Contains(s.Id))
                .ToListAsync();
        }

        public async Task<List<Service>> GetByBusinessIdAsync(Guid businessId)
        {
            return await _context.Services
                .Where(s => s.BusinessId == businessId)
                .ToListAsync();
        }
        public async Task<Service?> GetByIdAndBusinessAsync(Guid serviceId, Guid businessId)
        {
            return await _context.Services
                .FirstOrDefaultAsync(s => s.Id == serviceId && s.BusinessId == businessId);
        }

    }
}
