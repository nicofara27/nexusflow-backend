namespace NexusFlow.Models.Entities
{
    public class EmployeeSchedule
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public UserBusiness Employee { get; set; } = null!;
        public DayOfWeek DayOfWeek { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
