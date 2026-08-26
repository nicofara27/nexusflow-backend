namespace NexusFlow.Models.DTOs.Appointment
{
    public class AppointmentRequest
    {
        public Guid EmployeeId { get; set; }
        public Guid ServiceId { get; set; }
        public DateTime StartDate { get; set; }
    }
}
