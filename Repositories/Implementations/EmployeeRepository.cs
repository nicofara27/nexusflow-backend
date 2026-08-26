using Microsoft.EntityFrameworkCore;
using NexusFlow.Data;
using NexusFlow.Models.Entities;
using NexusFlow.Repositories.Interfaces;

namespace NexusFlow.Repositories.Implementations
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly AppDbContext _context;

        public EmployeeRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<EmployeeSchedule>> GetScheduleByUserBusinessIdAsync(Guid userBusinessId)
        {
            return await _context.EmployeeSchedules
                .Where(s => s.UserBusinessId == userBusinessId)
                .ToListAsync();
        }

        public async Task<EmployeeSchedule?> GetScheduleByUserBusinessIdAndDayAsync(Guid userBusinessId, DayOfWeek day)
        {
            return await _context.EmployeeSchedules
                .FirstOrDefaultAsync(es => es.UserBusinessId == userBusinessId && es.DayOfWeek == day && es.IsActive);
        }

        public void Add(EmployeeSchedule schedule)
        {
            _context.EmployeeSchedules.Add(schedule);
        }
    }
}
