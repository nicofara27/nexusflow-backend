using NexusFlow.Models.Entities;
using NexusFlow.Models.Enums;

namespace NexusFlow.Models.DTOs.Appointment
{
    public class AppointmentResponse
    {
        public Guid Id { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public AppointmentStatus Status { get; set; }
        public int Price { get; set; }
    }
}
