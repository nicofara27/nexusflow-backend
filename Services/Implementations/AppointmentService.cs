using NexusFlow.Models.DTOs.Appointment;
using NexusFlow.Models.Entities;
using NexusFlow.Models.Enums;
using NexusFlow.Repositories.Interfaces;
using NexusFlow.Services.Interfaces;

namespace NexusFlow.Services.Implementations
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IUserBusinessRepository _userBusinessRepository;
        private readonly IServiceRepository _serviceRepository;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IEmployeeRepository _employeeRepository;
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
            _employeeRepository = employeeRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<AppointmentResponse> CreateAppointmentAsync(Guid clientId, AppointmentRequest dto)
        {
            var employee = await _userBusinessRepository.GetEmployeeByIdWithDetailsAsync(dto.EmployeeId);
            if (employee == null || !employee.IsActive) throw new Exception("Empleado no encontrado.");

            var service = await _serviceRepository.GetByIdAndBusinessAsync(dto.ServiceId, employee.BusinessId);
            if (service == null || !service.IsActive) throw new Exception("Servicio no encontrado.");

            var endDate = dto.StartDate.AddMinutes(service.Duration);

            ValidateEmployeeServiceAndSchedule(employee, service, dto.StartDate.DayOfWeek);

            var startTime = TimeOnly.FromDateTime(dto.StartDate);
            var endTime = TimeOnly.FromDateTime(endDate);
            var schedule = employee.Schedules.First(s => s.DayOfWeek == dto.StartDate.DayOfWeek && s.IsActive);
            if (startTime < schedule.StartTime || endTime > schedule.EndTime)
                throw new Exception("El horario está fuera del horario laboral del empleado.");

            var hasConflict = await _appointmentRepository.HasConflictAsync(dto.EmployeeId, dto.StartDate, endDate);
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

            _appointmentRepository.Add(appointment);
            await _unitOfWork.SaveChangesAsync();

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
            var appointment = await _appointmentRepository.GetWithDetailsByIdAsync(appointmentId);
            if (appointment == null) throw new Exception("Turno no encontrado.");

            var isClient = appointment.ClientId == userId;
            var isEmployee = appointment.Employee.UserId == userId;
            var isAdmin = await _userBusinessRepository.IsAdminAsync(userId, appointment.BusinessId);

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
            _appointmentRepository.UpdateStatus(appointment);
            await _unitOfWork.SaveChangesAsync();

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

            return appointments.Select(a => new AppointmentResponse
            {
                Id = a.Id,
                ServiceName = a.Service.Name,
                EmployeeName = $"{a.Employee.User.FirstName} {a.Employee.User.LastName}",
                StartDate = a.StartDate,
                EndDate = a.EndDate,
                Status = a.Status,
                Price = a.Service.Price
            }).ToList();
        }

        public async Task<List<AvailableTimeResponse>> GetEmployeeAvailabilityAsync(Guid userBusinessId, Guid serviceId, DateOnly date)
        {
            var employee = await _userBusinessRepository.GetEmployeeByIdWithDetailsAsync(userBusinessId);
            if (employee == null || !employee.IsActive) throw new Exception("Empleado no encontrado.");

            var service = await _serviceRepository.GetByServiceIdAsync(serviceId);
            if (service == null || !service.IsActive) throw new Exception("Servicio no encontrado.");

            if (!employee.ServiceAssignment.Any(sa => sa.ServiceId == service.Id)) throw new Exception("El empleado no realiza este servicio.");

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
                throw new Exception("El empleado no realiza este servicio.");

            var schedule = employee.Schedules
                .FirstOrDefault(s => s.DayOfWeek == dayOfWeek && s.IsActive);
            if (schedule == null) throw new Exception("El empleado no trabaja ese día.");
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