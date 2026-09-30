using Microsoft.EntityFrameworkCore;
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

        public async Task<List<Business>> GetPublicBusinessesAsync()
        {
            return await _context.Business
                .AsNoTracking()
                .Include(b => b.BusinessCategory)
                .Include(b => b.Images)
                .ToListAsync();
        }

        public async Task<Business?> GetPublicByIdAsync(Guid businessId)
        {
            return await _context.Business
                .AsNoTracking()
                .Include(b => b.BusinessCategory)
                .Include(b => b.Images)
                .Include(b => b.Schedules)
                .Include(b => b.ServiceCategories)
                .Include(b => b.Services)
                .Include(b => b.Reviews)
                    .ThenInclude(r => r.Appointment)
                    .ThenInclude(a => a.Client)
                .Include(b => b.UserBusinesses)
                    .ThenInclude(ub => ub.User)
                .Include(b => b.UserBusinesses)
                    .ThenInclude(ub => ub.PortfolioImages)
                .AsSplitQuery()
                .FirstOrDefaultAsync(b => b.Id == businessId);
        }
    }
}
