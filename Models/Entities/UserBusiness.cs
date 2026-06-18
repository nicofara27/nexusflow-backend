using NexusFlow.Models.Enums;

namespace NexusFlow.Models.Entities
{
    public class UserBusiness
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public Guid BusinessId { get; set; }
        public Business Business { get; set; } = null!;
        public BusinessRole Role { get; set; }
        public ICollection<EmployeeSchedule> Schedules { get; set; } = [];
        public ICollection<Appointment> Appointments { get; set; } = [];
    }
}
