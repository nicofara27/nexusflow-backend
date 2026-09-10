using Microsoft.EntityFrameworkCore;
using NexusFlow.Data;

public class BusinessScheduleRepository : IBusinessScheduleRepository
{
    private readonly AppDbContext _context;

    public BusinessScheduleRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<BusinessSchedule>> GetByBusinessIdAsync(
        Guid businessId)
    {
        return await _context.BusinessSchedules
            .Where(bs => bs.BusinessId == businessId)
            .OrderBy(bs => bs.DayOfWeek)
            .ToListAsync();
    }

    public void Add(BusinessSchedule schedule)
    {
        _context.BusinessSchedules.Add(schedule);
    }

    public void Remove(BusinessSchedule schedule)
    {
        _context.BusinessSchedules.Remove(schedule);
    }
}