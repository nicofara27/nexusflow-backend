using NexusFlow.Models.DTOs.Appointment;

namespace NexusFlow.Services.Interfaces
{
    public interface IAppointmentService
    {
        Task<AppointmentResponse> CreateAppointmentAsync(Guid clientId, AppointmentRequest dto);
        Task<AppointmentResponse> ConfirmAppointmentAsync(string token);
        Task<AppointmentResponse> CancelAppointmentAsync(Guid userId, Guid appointmentId);
        Task<AppointmentResponse> CompleteAppointmentAsync(Guid userId, Guid appointmentId);
        Task<List<AppointmentResponse>> GetManagedAppointmentsAsync(Guid userId, Guid? employeeId = null);
        Task<List<ClientAppointmentResponse>> GetClientAppointmentsAsync(Guid clientId);
        Task<List<AvailableTimeResponse>> GetEmployeeAvailabilityAsync(Guid userBusinessId, Guid serviceId, DateOnly date);
    }
}
