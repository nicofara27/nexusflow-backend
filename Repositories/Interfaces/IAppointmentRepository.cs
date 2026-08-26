using NexusFlow.Models.DTOs;
using NexusFlow.Models.Entities;
using NexusFlow.Models.Enums;

namespace NexusFlow.Repositories.Interfaces
{
    public interface IAppointmentRepository
    {
        void Add(Appointment appointment);
        void UpdateStatus(Appointment appointment);
        Task<List<Appointment>> GetByClientIdAsync(Guid clientId);
        Task<List<Appointment>> GetByEmployeeIdAsync(Guid employeeId);
        Task<List<Appointment>> GetByBusinessIdAsync(Guid businessId);
        Task<List<Appointment>> GetByBusinessAndEmployeeIdAsync(Guid businessId, Guid employeeId);
        Task<List<Appointment>> GetByUserBusinessIdAndDate(Guid userBusinessId, DateOnly date);
        Task<bool> HasConflictAsync(Guid employeeId, DateTime startDate,  DateTime endDate);
        Task<Appointment?> GetWithDetailsByIdAsync(Guid appointmentId);
    }
}
