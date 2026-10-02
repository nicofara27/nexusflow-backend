using NexusFlow.Exceptions;
using NexusFlow.Models.Entities;
using NexusFlow.Repositories.Interfaces;
using NexusFlow.Services.Implementations;
using NSubstitute;
using Xunit;

namespace NexusFlow.Tests.Services;

public class AppointmentServiceTests
{
    private readonly IUserBusinessRepository _userBusinessRepository;
    private readonly IServiceRepository _serviceRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly AppointmentService _appointmentService;

    private readonly Guid _employeeId = Guid.NewGuid();
    private readonly Guid _serviceId = Guid.NewGuid();
    private readonly Guid _businessId = Guid.NewGuid();

    private readonly DateOnly _date = new(2026, 10, 5);

    public AppointmentServiceTests()
    {
        _userBusinessRepository =
            Substitute.For<IUserBusinessRepository>();

        _serviceRepository =
            Substitute.For<IServiceRepository>();

        _appointmentRepository =
            Substitute.For<IAppointmentRepository>();

        _employeeRepository =
            Substitute.For<IEmployeeRepository>();

        _unitOfWork =
            Substitute.For<IUnitOfWork>();

        _appointmentService = new AppointmentService(
            _userBusinessRepository,
            _serviceRepository,
            _appointmentRepository,
            _employeeRepository,
            _unitOfWork);
    }

    [Fact]
    public async Task GetEmployeeAvailabilityAsync_WhenEmployeeDoesNotWorkThatDay_ReturnsEmptyList()
    {
        var employee = new UserBusiness
        {
            Id = _employeeId,
            BusinessId = _businessId,
            IsActive = true,
            ServiceAssignment =
            [
                new ServiceAssignment
            {
                ServiceId = _serviceId,
            },
        ],
            Schedules =
            [
                new EmployeeSchedule
            {
                DayOfWeek = DayOfWeek.Tuesday,
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(18, 0),
                IsActive = true,
            },
        ],
        };

        var service = new Service
        {
            Id = _serviceId,
            BusinessId = _businessId,
            Name = "Corte",
            Duration = 30,
            Price = 10000,
            IsActive = true,
        };

        _userBusinessRepository
            .GetEmployeeByIdWithDetailsAsync(_employeeId)
            .Returns(employee);

        _serviceRepository
            .GetByServiceIdAsync(_serviceId)
            .Returns(service);

        var result =
            await _appointmentService.GetEmployeeAvailabilityAsync(
                _employeeId,
                _serviceId,
                _date);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetEmployeeAvailabilityAsync_WhenEmployeeDoesNotExist_ThrowsException()
    {
        _userBusinessRepository
            .GetEmployeeByIdWithDetailsAsync(_employeeId)
            .Returns((UserBusiness?)null);

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _appointmentService.GetEmployeeAvailabilityAsync(
                _employeeId,
                _serviceId,
                _date));

        Assert.Equal("Empleado no encontrado.", exception.Message);
    }

    [Fact]
    public async Task GetEmployeeAvailabilityAsync_WhenEmployeeIsInactive_ThrowsException()
    {
        var employee = new UserBusiness
        {
            Id = _employeeId,
            IsActive = false,
        };

        _userBusinessRepository
            .GetEmployeeByIdWithDetailsAsync(_employeeId)
            .Returns(employee);

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _appointmentService.GetEmployeeAvailabilityAsync(
                _employeeId,
                _serviceId,
                _date));

        Assert.Equal("Empleado no encontrado.", exception.Message);
    }
}