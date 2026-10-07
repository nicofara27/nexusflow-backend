using NexusFlow.Models.Entities;
using NexusFlow.Repositories.Interfaces;
using NexusFlow.Services.Implementations;
using NexusFlow.Models.DTOs.Appointment;
using NSubstitute;

namespace NexusFlow.Tests.Services;

public abstract class AppointmentServiceTestBase
{
    protected readonly IUserBusinessRepository _userBusinessRepository;
    protected readonly IServiceRepository _serviceRepository;
    protected readonly IAppointmentRepository _appointmentRepository;
    protected readonly IEmployeeRepository _employeeRepository;
    protected readonly IUnitOfWork _unitOfWork;

    protected readonly AppointmentService _appointmentService;

    protected readonly Guid _employeeId = Guid.NewGuid();
    protected readonly Guid _serviceId = Guid.NewGuid();
    protected readonly Guid _businessId = Guid.NewGuid();
    protected readonly Guid _clientId = Guid.NewGuid();

    protected readonly DateOnly _date = new(2026, 10, 5);

    protected AppointmentServiceTestBase()
    {
        _userBusinessRepository = Substitute.For<IUserBusinessRepository>();
        _serviceRepository = Substitute.For<IServiceRepository>();
        _appointmentRepository = Substitute.For<IAppointmentRepository>();
        _employeeRepository = Substitute.For<IEmployeeRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();

        _unitOfWork
        .ExecuteInTransactionAsync(
            Arg.Any<Func<Task<AppointmentResponse>>>())
        .Returns(callInfo =>
        {
            var action =
                callInfo.ArgAt<Func<Task<AppointmentResponse>>>(0);

            return action();
        });

        _appointmentService = new AppointmentService(
            _userBusinessRepository,
            _serviceRepository,
            _appointmentRepository,
            _employeeRepository,
            _unitOfWork);
    }

    protected UserBusiness CreateEmployee(bool isActive = true)
    {
        return new UserBusiness
        {
            Id = _employeeId,
            BusinessId = _businessId,
            IsActive = isActive,
            User = new User
            {
                FirstName = "Juan",
                LastName = "Pérez"
            },
            ServiceAssignment = [],
            Schedules = []
        };
    }

    protected Service CreateService(bool isActive = true)
    {
        return new Service
        {
            Id = _serviceId,
            BusinessId = _businessId,
            Name = "Corte",
            Duration = 30,
            Price = 10000,
            IsActive = isActive
        };
    }
}