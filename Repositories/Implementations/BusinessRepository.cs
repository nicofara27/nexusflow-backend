using NexusFlow.Data;
using NexusFlow.Models.DTOs;
using NexusFlow.Models.Entities;
using NexusFlow.Repositories.Interfaces;

namespace NexusFlow.Repositories.Implementations
{
    public class BusinessRepository : IBusinessRepository
    {
        private readonly AppDbContext _context;

        public BusinessRepository(AppDbContext context)
        {
            _context = context;
        }

        public void Add(Business business)
        {
            _context.Business.Add(business);
        }
        public async Task<Business?> GetByIdAsync(Guid businessId)
        {
            return await _context.Business.FindAsync(businessId);
        }
    }
}
