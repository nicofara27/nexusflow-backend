using NexusFlow.Models.Entities;

namespace NexusFlow.Repositories.Interfaces
{
    public interface IEmployeeRepository
    {
        Task<List<EmployeeSchedule>> GetScheduleByUserBusinessIdAsync(Guid userBusinessId);
        Task<EmployeeSchedule?> GetScheduleByUserBusinessIdAndDayAsync(Guid userBusinessId, DayOfWeek day);
        void Add(EmployeeSchedule employeeSchedule);
    }
}
