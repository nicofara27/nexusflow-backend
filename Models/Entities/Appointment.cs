using NexusFlow.Models.Enums;

namespace NexusFlow.Models.Entities
{
    public class Appointment
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public UserBusiness Employee { get; set; } = null!;
        public Guid ClientId {  get; set; }
        public User Client { get; set; } = null!;
        public Guid ServiceId {  get; set; }
        public Service Service { get; set; } = null!;
        public Guid BusinessId { get; set; }
        public Business Business { get; set; } = null!;
        public Review? Review { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
    }
}
