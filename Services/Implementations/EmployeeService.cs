using NexusFlow.Models.Entities;
using NexusFlow.Models.Enums;
using NexusFlow.Repositories.Interfaces;
using NexusFlow.Services.Interfaces;
using NexusFlow.Helpers;
using NexusFlow.Models.DTOs.Employee;

namespace NexusFlow.Services.Implementations
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IBusinessRepository _businessRepository;
        private readonly IUserBusinessRepository _userBusinessRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IServiceRepository _serviceRepository;
        private readonly IServiceAssignmentRepository _serviceAssignmentRepository;
        private readonly IUnitOfWork _unitOfWork;
        public EmployeeService(
            IAppointmentRepository appointmentRepository,
            IBusinessRepository businessRepository,
            IUserBusinessRepository userBusinessRepository,
            IEmployeeRepository employeeRepository,
            IServiceRepository serviceRepository,
            IServiceAssignmentRepository serviceAssignmentRepository,
            IUnitOfWork unitOfWork)
        {
            _appointmentRepository = appointmentRepository;
            _businessRepository = businessRepository;
            _userBusinessRepository = userBusinessRepository;
            _employeeRepository = employeeRepository;
            _serviceRepository = serviceRepository;
            _serviceAssignmentRepository = serviceAssignmentRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<List<EmployeeScheduleResponse>> UpsertEmployeeScheduleAsync(Guid userBusinessId, EmployeeScheduleRequest dto, Guid adminId)
        {
            var adminUserBusiness = await _userBusinessRepository.GetByUserIdAsync(adminId, BusinessRole.Admin);
            if (adminUserBusiness == null)
                throw new Exception("No tenés permiso para modificar horarios.");

            var employee = await _userBusinessRepository.GetEmployeeByIdAsync(userBusinessId);
            if (employee == null || employee.BusinessId != adminUserBusiness.BusinessId)
                throw new Exception("Empleado no encontrado.");


            var existingSchedules = await _employeeRepository.GetScheduleByUserBusinessIdAsync(userBusinessId);

            var responses = new List<EmployeeScheduleResponse>();

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                foreach (var item in dto.Schedules)
                {
                    var existing = existingSchedules.FirstOrDefault(s => s.DayOfWeek == item.DayOfWeek);

                    EmployeeSchedule schedule;

                    if (existing != null)
                    {
                        existing.StartTime = item.StartTime;
                        existing.EndTime = item.EndTime;
                        existing.IsActive = item.IsActive;

                        schedule = existing;
                    }
                    else
                    {
                        schedule = new EmployeeSchedule
                        {
                            UserBusinessId = userBusinessId,
                            DayOfWeek = item.DayOfWeek,
                            StartTime = item.StartTime,
                            EndTime = item.EndTime,
                            IsActive = item.IsActive
                        };
                        _employeeRepository.Add(schedule);
                    }
                    responses.Add(new EmployeeScheduleResponse
                    {
                        DayOfWeek = schedule.DayOfWeek,
                        StartTime = item.StartTime,
                        EndTime = item.EndTime,
                        IsActive = item.IsActive
                    });
                }
                await _unitOfWork.SaveChangesAsync();
            });

            return responses;
        }

        public async Task<List<EmployeeScheduleResponse>> GetEmployeeScheduleAsync(Guid userBusinessId)
        {
            var schedule = await _employeeRepository.GetScheduleByUserBusinessIdAsync(userBusinessId);

            return schedule.Select(s => new EmployeeScheduleResponse
            {
                DayOfWeek = s.DayOfWeek,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                IsActive = s.IsActive
            }).ToList();
        }

        public async Task<GetEmployeeResponse> GetEmployeeAsync(Guid userBusinessId, Guid requesterId)
        {
            var userBusiness = await _userBusinessRepository.GetEmployeeByIdWithDetailsAsync(userBusinessId);
            if (userBusiness == null) throw new Exception("No se encontro el empleado.");

            var isAllowed = await _userBusinessRepository.IsAdminOrEmployeeAsync(requesterId, userBusiness.BusinessId);
            if (!isAllowed) throw new Exception("No tienes los permisos necesarios.");

            return new GetEmployeeResponse
            {
                Id = userBusiness.Id,
                FirstName = userBusiness.User.FirstName,
                LastName = userBusiness.User.LastName,
                IsActive = userBusiness.IsActive,
                Services = userBusiness.ServiceAssignment.Select(sa => new EmployeeServiceDTO
                {
                    Id = sa.Service.Id,
                    Name = sa.Service.Name
                }).ToList()
            };
        }

        public async Task<List<GetEmployeeResponse>> GetEmployeesByBusinessIdAsync(Guid requesterId)
        {
            var requester = await _userBusinessRepository.GetByUserIdAsync(requesterId, BusinessRole.Admin);
            if (requester == null) throw new Exception("No tienes los permisos necesarios.");

            var employees = await _userBusinessRepository.GetEmployeesByBusinessIdAsync(requester.BusinessId);

            return employees.Select(e => new GetEmployeeResponse
            {
                Id = e.Id,
                FirstName = e.User.FirstName,
                LastName = e.User.LastName,
                IsActive = e.IsActive,
                Services = e.ServiceAssignment.Select(sa => new EmployeeServiceDTO
                {
                    Id = sa.Service.Id,
                    Name = sa.Service.Name
                }).ToList(),
            }).ToList();
        }

        public async Task<List<EmployeeSummaryResponse>> GetEmployeesByServiceIdAsync(Guid serviceId)
        {
            var service = await _serviceRepository.GetByServiceIdAsync(serviceId);
            if (service == null) throw new Exception("Servicio no encontrado.");

            var employees = await _userBusinessRepository.GetEmployeesByServiceIdAsync(serviceId, service.BusinessId);

            return employees.Select(e => new EmployeeSummaryResponse
            {
                Id = e.Id,
                FirstName = e.User.FirstName,
                LastName = e.User.LastName,
            }).ToList();
        }

        public async Task<UpdateEmployeeResponse> UpdateEmployeeAsync(Guid userBusinessId, Guid requesterId, UpdateEmployeeRequest dto)
        {
            var userBusiness = await _userBusinessRepository.GetEmployeeByIdWithDetailsAsync(userBusinessId);
            if (userBusiness == null) throw new Exception("Usuario no encontrado.");

            var isAllowed = await _userBusinessRepository.IsAdminOrEmployeeAsync(requesterId, userBusiness.BusinessId);
            if (!isAllowed) throw new Exception("No tienes los permisos necesarios.");

            var services = await _serviceRepository.GetByIdsAsync(dto.ServiceIds);
            if (services.Count != dto.ServiceIds.Count) throw new Exception("Uno o más servicios no fueron encontrados.");

            var invalidServices = services.Where(s => s.BusinessId != userBusiness.BusinessId).ToList();
            if (invalidServices.Any()) throw new Exception("Uno o más servicios no pertenecen a este negocio.");

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                userBusiness.User.FirstName = dto.FirstName;
                userBusiness.User.LastName = dto.LastName;
                userBusiness.IsActive = dto.IsActive;

                await _serviceAssignmentRepository.DeleteByUserBusinessIdAsync(userBusinessId);

                var newServices = services.Select(s => new ServiceAssignment
                {
                    UserBusinessId = userBusinessId,
                    ServiceId = s.Id
                }).ToList();

                await _serviceAssignmentRepository.AddRangeAsync(newServices);

                await _unitOfWork.SaveChangesAsync();
            });


            return new UpdateEmployeeResponse
            {
                FirstName = userBusiness.User.FirstName,
                LastName = userBusiness.User.LastName,
                IsActive = userBusiness.IsActive,
                Services = services.Select(s => new EmployeeServiceDTO
                {
                    Id = s.Id,
                    Name = s.Name,
                }).ToList()

            };
        }
    }
}
