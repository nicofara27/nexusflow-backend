using NexusFlow.Exceptions;
using NexusFlow.Models.DTOs.Appointment;
using NexusFlow.Models.Entities;
using NexusFlow.Models.Enums;
using NexusFlow.Repositories.Interfaces;
using NexusFlow.Services.Interfaces;
using NexusFlow.Services.mappers;
using NexusFlow.Services.Helpers;

namespace NexusFlow.Services.Implementations
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IUserBusinessRepository _userBusinessRepository;
        private readonly IServiceRepository _serviceRepository;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AppointmentService(
            IUserBusinessRepository userBusinessRepository,
            IServiceRepository serviceRepository,
            IAppointmentRepository appointmentRepository,
            IEmployeeRepository employeeRepository,
            IUnitOfWork unitOfWork)
        {
            _userBusinessRepository = userBusinessRepository;
            _serviceRepository = serviceRepository;
            _appointmentRepository = appointmentRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<AppointmentResponse> CreateAppointmentAsync(Guid clientId, AppointmentRequest dto)
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                await _userBusinessRepository
                    .LockEmployeeForUpdateAsync(dto.EmployeeId);

                var employee = await _userBusinessRepository
                    .GetEmployeeByIdWithDetailsAsync(dto.EmployeeId);

                if (employee == null || !employee.IsActive)
                    throw new NotFoundException("Empleado no encontrado.");

                var service = await _serviceRepository
                    .GetByIdAndBusinessAsync(
                        dto.ServiceId,
                        employee.BusinessId);

                if (service == null || !service.IsActive)
                    throw new NotFoundException("Servicio no encontrado.");

                var endDate =
                    dto.StartDate.AddMinutes(service.Duration);

                ValidateEmployeeServiceAndSchedule(
                    employee,
                    service,
                    dto.StartDate.DayOfWeek);

                var startTime = TimeOnly.FromDateTime(dto.StartDate);
                var endTime = TimeOnly.FromDateTime(endDate);

                var schedule = employee.Schedules.First(
                    s => s.DayOfWeek == dto.StartDate.DayOfWeek
                         && s.IsActive);

                if (startTime < schedule.StartTime ||
                    endTime > schedule.EndTime)
                {
                    throw new BusinessRuleException(
                        "El horario está fuera del horario laboral del empleado.");
                }

                var hasConflict =
                    await _appointmentRepository.HasConflictAsync(
                        dto.EmployeeId,
                        dto.StartDate,
                        endDate);

                if (hasConflict)
                {
                    throw new ConflictException(
                        "El empleado ya tiene un turno en ese horario.");
                }

                var confirmationToken = ConfirmationTokenHelper.Generate();
                var confirmationTokenHash = ConfirmationTokenHelper.Hash(confirmationToken);

                var appointment = new Appointment
                {
                    ClientId = clientId,
                    EmployeeId = dto.EmployeeId,
                    ServiceId = dto.ServiceId,
                    BusinessId = employee.BusinessId,
                    StartDate = dto.StartDate,
                    EndDate = endDate,
                    Status = AppointmentStatus.Pending,
                    ConfirmationTokenHash = confirmationTokenHash,
                    ConfirmationTokenExpiresAt = DateTime.UtcNow.AddHours(1)
                };

                _appointmentRepository.Add(appointment);

                await _unitOfWork.SaveChangesAsync();

                return AppointmentMapper.ToResponse(appointment, service, employee);
            });
        }

        public async Task<AppointmentResponse> ConfirmAppointmentAsync(string token)
        {
            var tokenHash = ConfirmationTokenHelper.Hash(token);

            var appointment = await _appointmentRepository.GetByConfirmationTokenHashAsync(tokenHash);
            if (appointment == null) throw new NotFoundException("Token de confirmación inválido.");

            if (appointment.Status != AppointmentStatus.Pending) throw new BusinessRuleException("El turno ya no está pendiente de confirmación.");

            if(appointment.ConfirmationTokenExpiresAt == null || 
                appointment.ConfirmationTokenExpiresAt < DateTime.UtcNow)
            {
                throw new BusinessRuleException("El token de confirmación expiró.");
            }

            appointment.Status = AppointmentStatus.Confirmed;
            appointment.ConfirmationTokenHash = null;
            appointment.ConfirmationTokenExpiresAt = null;

            _appointmentRepository.UpdateStatus(appointment);
            await _unitOfWork.SaveChangesAsync();

            return AppointmentMapper.ToResponse(appointment);
        }

        public async Task<AppointmentResponse> CancelAppointmentAsync(Guid userId, Guid appointmentId)
        {
            var appointment = await _appointmentRepository.GetWithDetailsByIdAsync(appointmentId);

            if (appointment == null) throw new NotFoundException("Turno no encontrado.");

            var isClient = appointment.ClientId == userId;
            var isEmployee = appointment.Employee.UserId == userId;
            var isAdmin = await _userBusinessRepository.IsAdminAsync(userId, appointment.BusinessId);

            if (!isClient && !isEmployee && !isAdmin)
                throw new ForbiddenException("No tenés permiso para cancelar este turno.");

            if (appointment.Status is not AppointmentStatus.Pending and not AppointmentStatus.Confirmed)
            {
                throw new BusinessRuleException($"No se puede cancelar un turno en estado {appointment.Status}.");
            }

            appointment.Status = AppointmentStatus.Cancelled;

            _appointmentRepository.UpdateStatus(appointment);
            await _unitOfWork.SaveChangesAsync();

            return AppointmentMapper.ToResponse(appointment);
        }

        public async Task<AppointmentResponse> CompleteAppointmentAsync(Guid userId, Guid appointmentId)
        {
            var appointment = await _appointmentRepository.GetWithDetailsByIdAsync(appointmentId);

            if (appointment == null) throw new NotFoundException("Turno no encontrado.");

            var isEmployee = appointment.Employee.UserId == userId;

            var isAdmin = await _userBusinessRepository.IsAdminAsync(
                userId,
                appointment.BusinessId);

            if (!isEmployee && !isAdmin)
                throw new ForbiddenException("No tenés permiso para completar este turno.");

            if (appointment.Status != AppointmentStatus.Confirmed)
            {
                throw new BusinessRuleException("Solo se puede completar un turno confirmado.");
            }

            appointment.Status = AppointmentStatus.Completed;

            _appointmentRepository.UpdateStatus(appointment);
            await _unitOfWork.SaveChangesAsync();

            return AppointmentMapper.ToResponse(appointment);
        }

        public async Task<List<AppointmentResponse>> GetAppointmentsAsync(Guid userId, Guid? employeeId = null)
        {
            var userBusiness = await _userBusinessRepository.GetByUserIdAsync(userId);

            List<Appointment> appointments;

            if (userBusiness != null && userBusiness.Role == BusinessRole.Admin)
            {
                appointments = employeeId.HasValue
                    ? await _appointmentRepository.GetByBusinessAndEmployeeIdAsync(userBusiness.BusinessId, employeeId.Value)
                    : await _appointmentRepository.GetByBusinessIdAsync(userBusiness.BusinessId);
            }
            else if (userBusiness != null && userBusiness.Role == BusinessRole.Employee)
            {
                appointments = await _appointmentRepository.GetByEmployeeIdAsync(userBusiness.Id);
            }
            else
            {
                appointments = await _appointmentRepository.GetByClientIdAsync(userId);
            }

            return appointments.Select(AppointmentMapper.ToResponse).ToList();
        }

        public async Task<List<AvailableTimeResponse>> GetEmployeeAvailabilityAsync(Guid userBusinessId, Guid serviceId, DateOnly date)
        {
            var employee = await _userBusinessRepository.GetEmployeeByIdWithDetailsAsync(userBusinessId);
            if (employee == null || !employee.IsActive) throw new NotFoundException("Empleado no encontrado.");

            var service = await _serviceRepository.GetByServiceIdAsync(serviceId);
            if (service == null || !service.IsActive) throw new NotFoundException("Servicio no encontrado.");

            if (!employee.ServiceAssignment.Any(sa => sa.ServiceId == service.Id)) throw new BusinessRuleException("El empleado no realiza este servicio.");

            var schedule = employee.Schedules.FirstOrDefault(s =>s.DayOfWeek == date.DayOfWeek && s.IsActive);
            if (schedule == null) return [];

            var appointments = await _appointmentRepository.GetByUserBusinessIdAndDate(userBusinessId, date);

            var slotInterval = TimeSpan.FromMinutes(10);
            var serviceDuration = TimeSpan.FromMinutes(service.Duration);

            return GenerateAvailability(date, schedule!, appointments, serviceDuration, slotInterval);
        }

        private void ValidateEmployeeServiceAndSchedule(
            UserBusiness employee,
            Service service,
            DayOfWeek dayOfWeek)
        {
            if (!employee.ServiceAssignment.Any(sa => sa.ServiceId == service.Id))
                throw new BusinessRuleException("El empleado no realiza este servicio.");

            var schedule = employee.Schedules
                .FirstOrDefault(s => s.DayOfWeek == dayOfWeek && s.IsActive);
            if (schedule == null) throw new BusinessRuleException("El empleado no trabaja ese día.");
        }

        private static List<AvailableTimeResponse> GenerateAvailability(
            DateOnly date,
            EmployeeSchedule schedule,
            IReadOnlyCollection<Appointment> appointments,
            TimeSpan serviceDuration,
            TimeSpan slotInterval)
        {
            var availableTimes = new List<AvailableTimeResponse>();

            var currentTime = schedule.StartTime;

            while (currentTime.Add(serviceDuration) <= schedule.EndTime)
            {
                var start = date.ToDateTime(currentTime);
                var end = start.Add(serviceDuration);

                var occupied = appointments.Any(a =>
                    start < a.EndDate &&
                    end > a.StartDate);

                if (!occupied)
                {
                    availableTimes.Add(new AvailableTimeResponse
                    {
                        StartTime = currentTime,
                        EndTime = TimeOnly.FromDateTime(end)
                    });
                }

                currentTime = currentTime.Add(slotInterval);
            }

            return availableTimes;
        }
    }
}