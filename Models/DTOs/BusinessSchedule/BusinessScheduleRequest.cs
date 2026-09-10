using NexusFlow.Models.DTOs.BusinessSchedule;

public class BusinessScheduleRequest
{
    public List<BusinessDaySchedule> Days { get; set; } = [];
}