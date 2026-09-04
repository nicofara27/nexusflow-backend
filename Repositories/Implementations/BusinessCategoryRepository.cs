using Microsoft.EntityFrameworkCore;
using NexusFlow.Data;
using NexusFlow.Models.Entities;
using NexusFlow.Repositories.Interfaces;

namespace NexusFlow.Repositories.Implementations
{
    public class BusinessCategoryRepository : IBusinessCategoryRepository
    {
        private readonly AppDbContext _context;

        public BusinessCategoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<BusinessCategory?> GetByIdAsync(Guid businessCategoryId)
        {
            return await _context.BusinessCategories.FirstOrDefaultAsync(category => category.Id == businessCategoryId);
        }

        public async Task<List<BusinessCategory>> GetAllAsync()
        {
            return await _context.BusinessCategories
                .OrderBy(category =>  category.Name)
                .ToListAsync();
        }
    }
}
