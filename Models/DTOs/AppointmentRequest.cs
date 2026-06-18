using NexusFlow.Models.Entities;

namespace NexusFlow.Models.DTOs
{
    public class AppointmentRequest
    {
        public Guid EmployeeId { get; set; }
        public Guid ServiceId { get; set; }
        public DateTime StartDate { get; set; }
    }
}
