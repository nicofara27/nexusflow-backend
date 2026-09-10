namespace NexusFlow.Models.DTOs.BusinessSchedule
{
    public class BusinessScheduleResponse
    {
        public DayOfWeek DayOfWeek { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
    }
}
