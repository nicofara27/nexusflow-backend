using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusFlow.Data;
using NexusFlow.Models.DTOs;
using NexusFlow.Models.Entities;
using NexusFlow.Models.Enums;

namespace NexusFlow.Services
{
    public class AppointmentService
    {
        private readonly AppDbContext _context;

        public AppointmentService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<AppointmentResponse> CreateAppointmentAsync(Guid clientId, AppointmentRequest dto)
        {
            var employee = await _context.UsersBusiness
                .Include(ub => ub.User)
                .Include(ub => ub.Schedules)
                .FirstOrDefaultAsync(ub => ub.Id == dto.EmployeeId && ub.Role == BusinessRole.Employee);
            if (employee == null) throw new Exception("Empleado no encontrado.");

            var service = await _context.Services
                .FirstOrDefaultAsync(s => s.Id == dto.ServiceId && s.BusinessId == employee.BusinessId);
            if (service == null) throw new Exception("Servicio no encontrado.");

            var dayOfWeek = dto.StartDate.DayOfWeek;
            var schedule = employee.Schedules
                .FirstOrDefault(s => s.DayOfWeek == dayOfWeek && s.IsActive);
            if (schedule == null) throw new Exception("El empleado no trabaja en ese día.");

            var startTime = TimeOnly.FromDateTime(dto.StartDate);
            var endTime = startTime.AddMinutes(service.Duration);
            if (startTime < schedule.StartTime || endTime > schedule.EndTime)
                throw new Exception("El horario está fuera del horario laboral del empleado.");

            var endDate = dto.StartDate.AddMinutes(service.Duration);

            var hasConflict = await _context.Appointments
                .AnyAsync(a =>
                    a.EmployeeId == dto.EmployeeId &&
                    a.Status != AppointmentStatus.Cancelled &&
                    a.StartDate < endDate &&
                    a.EndDate > dto.StartDate
                );
            if (hasConflict) throw new Exception("El empleado ya tiene un turno en ese horario.");

            var appointment = new Appointment
            {
                ClientId = clientId,
                EmployeeId = dto.EmployeeId,
                ServiceId = dto.ServiceId,
                BusinessId = employee.BusinessId,
                StartDate = dto.StartDate,
                EndDate = endDate,
                Status = AppointmentStatus.Pending
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return new AppointmentResponse
            {
                Id = appointment.Id,
                ServiceName = service.Name,
                EmployeeName = $"{employee.User.FirstName} {employee.User.LastName}",
                StartDate = appointment.StartDate,
                EndDate = appointment.EndDate,
                Status = appointment.Status,
                Price = service.Price
            };
        }

        public async Task<AppointmentResponse> UpdateAppointmentStatusAsync(Guid userId, Guid appointmentId, AppointmentStatus newStatus)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Service)
                .Include(a => a.Employee)
                    .ThenInclude(e => e.User)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);
            if (appointment == null) throw new Exception("Turno no encontrado.");

            var isClient = appointment.ClientId == userId;
            var isEmployee = appointment.Employee.UserId == userId;
            var isAdmin = await _context.UsersBusiness
                .AnyAsync(ub => ub.UserId == userId &&
                                ub.BusinessId == appointment.BusinessId &&
                                ub.Role == BusinessRole.Admin);

            if (!isClient && !isEmployee && !isAdmin)
                throw new Exception("No tenés permiso para modificar este turno");

            var allowedTransitions = new Dictionary<AppointmentStatus, List<AppointmentStatus>>
            {
                { AppointmentStatus.Pending, [AppointmentStatus.Confirmed, AppointmentStatus.Cancelled] },
                { AppointmentStatus.Confirmed, [AppointmentStatus.Completed, AppointmentStatus.Cancelled] },
                { AppointmentStatus.Completed, [] },
                { AppointmentStatus.Cancelled, [] }
            };

            if (!allowedTransitions[appointment.Status].Contains(newStatus))
                throw new Exception($"No se puede cambiar el estado de {appointment.Status} a {newStatus}");

            appointment.Status = newStatus;
            await _context.SaveChangesAsync();

            return new AppointmentResponse
            {
                Id = appointment.Id,
                ServiceName = appointment.Service.Name,
                EmployeeName = $"{appointment.Employee.User.FirstName} {appointment.Employee.User.LastName}",
                StartDate = appointment.StartDate,
                EndDate = appointment.EndDate,
                Status = appointment.Status,
                Price = appointment.Service.Price
            };
        }

        public async Task<List<AppointmentResponse>> GetAppointmentsAsync(Guid userId, Guid? employeeId = null)
        {
            var userBusiness = await _context.UsersBusiness.FirstOrDefaultAsync(ub => ub.UserId == userId);

            IQueryable<Appointment> query = _context.Appointments
                .Include(a => a.Service)
                .Include(a => a.Employee)
                    .ThenInclude(e => e.User)
                .Include(a => a.Client);

            if (userBusiness != null && userBusiness.Role == BusinessRole.Admin)
            {
                query = query.Where(a => a.BusinessId == userBusiness.BusinessId);
                if (employeeId.HasValue)
                {
                    query = query.Where(a => a.EmployeeId == employeeId.Value);
                }
            }
            else if (userBusiness != null && userBusiness.Role == BusinessRole.Employee)
            {
                query = query.Where(a => a.EmployeeId == userBusiness.Id);
            }
            else
            {
                query = query.Where(a => a.ClientId == userId);
            }

            var appointments = await query
                .OrderByDescending(a => a.StartDate)
                .Select(a => new AppointmentResponse
                {
                    Id = a.Id,
                    ServiceName = a.Service.Name,
                    EmployeeName = $"{a.Employee.User.FirstName} {a.Employee.User.LastName}",
                    StartDate = a.StartDate,
                    EndDate = a.EndDate,
                    Status = a.Status,
                    Price = a.Service.Price
                })
                .ToListAsync();

            return appointments;

        }
    }
}


// Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjA5ZjRmZTI5LTdmZjYtNGQ1NS04ZmRlLTA4ZGVjZDMwNjJjOCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6Inp4Y0BnbWFpbC5jb20iLCJleHAiOjE3ODE3OTA4OTUsImlzcyI6Ik5leHVzRmxvd0FwaSIsImF1ZCI6Ik5leHVzRmxvd0Zyb250In0.NQ_GdQnpTxxLRn3KgWvfjdQpmxTErr994SKgGFfXSXc
