namespace NexusFlow.Models.DTOs.BusinessSchedule
{
    public class BusinessDaySchedule
    {
        public DayOfWeek DayOfWeek { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
    }
}
