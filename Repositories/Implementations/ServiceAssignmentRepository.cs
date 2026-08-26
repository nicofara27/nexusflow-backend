using Microsoft.EntityFrameworkCore;
using NexusFlow.Data;
using NexusFlow.Models.Entities;
using NexusFlow.Repositories.Interfaces;

namespace NexusFlow.Repositories.Implementations
{
    public class ServiceAssignmentRepository : IServiceAssignmentRepository
    {
        private readonly AppDbContext _context;

        public ServiceAssignmentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task DeleteByUserBusinessIdAsync(Guid userBusinessId)
        {
            var assignments = await _context.ServiceAssignment
                .Where(sa => sa.UserBusinessId == userBusinessId)
                .ToListAsync();

            _context.ServiceAssignment.RemoveRange(assignments);
        }

        public async Task AddRangeAsync(List<ServiceAssignment> assignments)
        {
            await _context.ServiceAssignment.AddRangeAsync(assignments);
        }
    }
}
