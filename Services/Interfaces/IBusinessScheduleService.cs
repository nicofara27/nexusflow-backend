using NexusFlow.Models.DTOs.BusinessSchedule;

public interface IBusinessScheduleService
{
    Task<List<BusinessScheduleResponse>> GetAsync(Guid userId);
    Task<List<BusinessScheduleResponse>> UpdateAsync(Guid userId, BusinessScheduleRequest request);
}