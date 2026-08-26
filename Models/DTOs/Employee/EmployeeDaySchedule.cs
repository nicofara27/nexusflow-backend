namespace NexusFlow.Models.DTOs.Employee
{
    public class EmployeeDaySchedule
    {
        public DayOfWeek DayOfWeek { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
    }
}
