using NexusFlow.Models.DTOs.Appointment;
using NexusFlow.Models.Enums;

namespace NexusFlow.Services.Interfaces
{
    public interface IAppointmentService
    {
        Task<AppointmentResponse> CreateAppointmentAsync(Guid clientId, AppointmentRequest dto);
        Task<AppointmentResponse> UpdateAppointmentStatusAsync(Guid userId, Guid appointmentId, AppointmentStatus newStatus);
        Task<List<AppointmentResponse>> GetAppointmentsAsync(Guid userId, Guid? employeeId = null);
        Task<List<AvailableTimeResponse>> GetEmployeeAvailabilityAsync(Guid userBusinessId, Guid serviceId, DateOnly date);
        
    }
}
