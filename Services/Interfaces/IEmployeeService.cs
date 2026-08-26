using NexusFlow.Models.DTOs.Employee;
using NexusFlow.Models.Entities;

namespace NexusFlow.Services.Interfaces
{
    public interface IEmployeeService
    {
        Task<List<EmployeeScheduleResponse>> UpsertEmployeeScheduleAsync(Guid userBusinessId, EmployeeScheduleRequest dto, Guid adminId);
        Task<List<EmployeeScheduleResponse>> GetEmployeeScheduleAsync(Guid userBusinessId);
        Task<GetEmployeeResponse> GetEmployeeAsync(Guid userBusinessId, Guid requesterId);
        Task<List<GetEmployeeResponse>> GetEmployeesByBusinessIdAsync(Guid requesterId);
        Task<List<EmployeeSummaryResponse>> GetEmployeesByServiceIdAsync(Guid serviceId);
        Task<UpdateEmployeeResponse> UpdateEmployeeAsync(Guid userBusinessId, Guid requesterId, UpdateEmployeeRequest dto);
    }
}
