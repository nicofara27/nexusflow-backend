using Microsoft.EntityFrameworkCore;
using NexusFlow.Data;
using NexusFlow.Models.DTOs;
using NexusFlow.Models.Entities;
using NexusFlow.Models.Enums;
using NexusFlow.Repositories.Interfaces;

namespace NexusFlow.Repositories.Implementations
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly AppDbContext _context;

        public AppointmentRepository(AppDbContext context)
        {
            _context = context;
        }
        private IQueryable<Appointment> GetBaseQuery()
        {
            return _context.Appointments
                .Include(a => a.Service)
                .Include(a => a.Employee)
                    .ThenInclude(e => e.User)
                .Include(a => a.Client);
        }

        public void Add(Appointment appointment)
        {
            _context.Appointments.Add(appointment);
        }

        public void UpdateStatus(Appointment appointment)
        {
            _context.Appointments.Update(appointment);
        }

        public async Task<List<Appointment>> GetByClientIdAsync(Guid clientId)
        {
            return await GetBaseQuery()
                .Where(a => a.ClientId == clientId)
                .ToListAsync();
        }

        public async Task<List<Appointment>> GetByEmployeeIdAsync(Guid employeeId)
        {
            return await GetBaseQuery()
                .Where(a => a.EmployeeId == employeeId)
                .ToListAsync();
        }

        public async Task<List<Appointment>> GetByBusinessIdAsync(Guid businessId)
        {
            return await GetBaseQuery()
                .Where(a => a.BusinessId == businessId)
                .ToListAsync();
        }

        public async Task<List<Appointment>> GetByBusinessAndEmployeeIdAsync(Guid businessId, Guid employeeId)
        {
            return await GetBaseQuery()
                .Where(a => a.BusinessId == businessId && a.EmployeeId == employeeId)
                .ToListAsync();
        }

        public async Task<List<Appointment>> GetByUserBusinessIdAndDate(Guid userBusinessId, DateOnly date)
        {
            var startOfDay = date.ToDateTime(TimeOnly.MinValue);
            var endOfDay = startOfDay.AddDays(1);

            return await _context.Appointments
                .Where(a =>
                    a.EmployeeId == userBusinessId &&
                    a.StartDate >= startOfDay &&
                    a.EndDate < endOfDay)
                .OrderBy(a => a.StartDate)
                .ToListAsync();
        }

        public async Task<bool> HasConflictAsync(Guid employeeId, DateTime startDate, DateTime endDate)
        {
            return await _context.Appointments
                .AnyAsync(a =>
                    a.EmployeeId == employeeId &&
                    a.Status != AppointmentStatus.Cancelled &&
                    a.StartDate < endDate &&
                    a.EndDate > startDate
                );
        }

        public async Task<Appointment?> GetWithDetailsByIdAsync(Guid appointmentId)
        {
            return await GetBaseQuery()
                .FirstOrDefaultAsync(a => a.Id == appointmentId);
        }
    }
}
