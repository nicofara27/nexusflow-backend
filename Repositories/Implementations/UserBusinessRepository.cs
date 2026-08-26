using Microsoft.EntityFrameworkCore;
using NexusFlow.Data;
using NexusFlow.Models.Entities;
using NexusFlow.Models.Enums;
using NexusFlow.Repositories.Interfaces;

namespace NexusFlow.Repositories.Implementations
{
    public class UserBusinessRepository : IUserBusinessRepository
    {
        private readonly AppDbContext _context;

        public UserBusinessRepository(AppDbContext context)
        {
            _context = context;
        }

        public void Add(UserBusiness userBusiness)
        {
            _context.UsersBusiness.Add(userBusiness);
        }

        public async Task<bool> IsAdminAsync(Guid userId, Guid businessId)
        {
            return await _context.UsersBusiness
                .AnyAsync(ub => ub.UserId == userId &&
                                ub.BusinessId == businessId &&
                                ub.Role == BusinessRole.Admin);
        }
        public async Task<bool> IsAdminOrEmployeeAsync(Guid userId, Guid businessId)
        {
            return await _context.UsersBusiness
                .AnyAsync(ub => ub.UserId == userId &&
                                ub.BusinessId == businessId &&(
                                ub.Role == BusinessRole.Admin ||
                                ub.Role == BusinessRole.Employee));
        }

        public async Task<UserBusiness?> GetEmployeeByIdAsync(Guid employeeId)
        {
            return await _context.UsersBusiness
                .FirstOrDefaultAsync(ub => ub.Id == employeeId && ub.Role == BusinessRole.Employee);
        }

        public async Task<UserBusiness?> GetEmployeeByIdWithDetailsAsync(Guid employeeId)
        {
            return await _context.UsersBusiness
                .Include(ub => ub.User)
                .Include(ub => ub.Schedules)
                .Include(ub => ub.ServiceAssignment)
                    .ThenInclude(sa => sa.Service)
                .FirstOrDefaultAsync(ub => ub.Id == employeeId && ub.Role == BusinessRole.Employee);
        }

        public async Task<UserBusiness?> GetByUserIdAsync(Guid adminId, BusinessRole? role = null)
        {
            return await _context.UsersBusiness
                .FirstOrDefaultAsync(ub =>
                    ub.UserId == adminId
                    && (role == null || ub.Role == role));
        }

        public async Task<UserBusiness?> GetByUserIdWithBusinessAsync(Guid userId)
        {
            return await _context.UsersBusiness
                .Include(ub => ub.Business)
                .FirstOrDefaultAsync(ub => ub.UserId == userId);
        }

        public async Task<Business?> GetBusinessByUserIdAsync(Guid userId)
        {
            return await _context.UsersBusiness
                .Where(ub => ub.UserId == userId)
                .Select(ub => ub.Business)
                .FirstOrDefaultAsync();
        }

        public async Task<List<UserBusiness>> GetEmployeesByBusinessIdAsync(Guid businessId)
        {
            return await _context.UsersBusiness
                .Where(ub => ub.BusinessId == businessId)
                .Include(ub => ub.User)
                .ToListAsync();
        }

        public async Task<List<UserBusiness>> GetEmployeesByServiceIdAsync(Guid serviceId, Guid businessId)
        {
            return await _context.UsersBusiness
                .Include(ub => ub.User)
                .Include(ub => ub.ServiceAssignment)
                    .ThenInclude(sa => sa.Service)
                .Where(ub =>
                    ub.BusinessId == businessId &&
                    ub.Role == BusinessRole.Employee &&
                    ub.IsActive &&
                    ub.ServiceAssignment.Any(sa => sa.ServiceId == serviceId))
                .ToListAsync();
        }
    }
}
