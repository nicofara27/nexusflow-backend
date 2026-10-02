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
    public async Task GetEmployeeAvailabilityAsync_WhenEmployeeDoesNotExist_ThrowsNotFoundException()
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
    public async Task GetEmployeeAvailabilityAsync_WhenEmployeeIsInactive_ThrowsNotFoundException()
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

    [Fact]
    public async Task GetEmployeeAvailabilityAsync_WhenServiceDoesNotExist_ThrowsNotFoundException()
    {
        var employee = new UserBusiness
        {
            Id = _employeeId,
            BusinessId = _businessId,
            IsActive = true,
        };

        _userBusinessRepository
            .GetEmployeeByIdWithDetailsAsync(_employeeId)
            .Returns(employee);

        _serviceRepository
            .GetByServiceIdAsync(_serviceId)
            .Returns((Service?)null);

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _appointmentService.GetEmployeeAvailabilityAsync(
                _employeeId,
                _serviceId,
                _date));

        Assert.Equal("Servicio no encontrado.", exception.Message);
    }

    [Fact]
    public async Task GetEmployeeAvailabilityAsync_WhenServicesIsInactive_ThrowsNotFoundException()
    {
        var employee = new UserBusiness
        {
            Id = _employeeId,
            BusinessId = _businessId,
            IsActive = true,
        };

        var service = new Service
        {
            Id = _serviceId,
            BusinessId = _businessId,
            Name = "Corte",
            Duration = 30,
            Price = 10000,
            IsActive = false,
        };

        _userBusinessRepository
            .GetEmployeeByIdWithDetailsAsync(_employeeId)
            .Returns(employee);

        _serviceRepository
            .GetByServiceIdAsync(_serviceId)
            .Returns(service);

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _appointmentService.GetEmployeeAvailabilityAsync(
                _employeeId,
                _serviceId,
                _date));

        Assert.Equal("Servicio no encontrado.", exception.Message);
    }

    [Fact]
    public async Task GetEmployeeAvailabilityAsync_WhenEmployeeDoesNotPerformService_ThrowsBusinessRuleException()
    {
        var employee = new UserBusiness
        {
            Id = _employeeId,
            BusinessId = _businessId,
            IsActive = true,
            ServiceAssignment = [],
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

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(
            () => _appointmentService.GetEmployeeAvailabilityAsync(
                _employeeId,
                _serviceId,
                _date));

        Assert.Equal(
            "El empleado no realiza este servicio.",
            exception.Message);
    }

    [Fact]
    public async Task GetEmployeeAvailabilityAsync_WhenScheduleIsAvailable_ReturnsTimeSlotsEveryTenMinutes()
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
                ServiceId = _serviceId
            }
            ],
            Schedules =
            [
                new EmployeeSchedule
            {
                DayOfWeek = _date.DayOfWeek,
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(10, 0),
                IsActive = true
            }
            ]
        };

        var service = new Service
        {
            Id = _serviceId,
            BusinessId = _businessId,
            Name = "Corte",
            Duration = 30,
            Price = 10000,
            IsActive = true
        };

        _userBusinessRepository
            .GetEmployeeByIdWithDetailsAsync(_employeeId)
            .Returns(employee);

        _serviceRepository
            .GetByServiceIdAsync(_serviceId)
            .Returns(service);

        _appointmentRepository
            .GetByUserBusinessIdAndDate(_employeeId, _date)
            .Returns([]);

        var result = await _appointmentService.GetEmployeeAvailabilityAsync(
            _employeeId,
            _serviceId,
            _date);

        Assert.Collection(
            result,
            slot =>
            {
                Assert.Equal(new TimeOnly(9, 0), slot.StartTime);
                Assert.Equal(new TimeOnly(9, 30), slot.EndTime);
            },
            slot =>
            {
                Assert.Equal(new TimeOnly(9, 10), slot.StartTime);
                Assert.Equal(new TimeOnly(9, 40), slot.EndTime);
            },
            slot =>
            {
                Assert.Equal(new TimeOnly(9, 20), slot.StartTime);
                Assert.Equal(new TimeOnly(9, 50), slot.EndTime);
            },
            slot =>
            {
                Assert.Equal(new TimeOnly(9, 30), slot.StartTime);
                Assert.Equal(new TimeOnly(10, 0), slot.EndTime);
            });
    }

    [Fact]
    public async Task GetEmployeeAvailabilityAsync_WhenAppointmentOverlapsSlots_ReturnsOnlyAvailableSlots()
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
                ServiceId = _serviceId
            }
            ],
            Schedules =
            [
                new EmployeeSchedule
            {
                DayOfWeek = _date.DayOfWeek,
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(10, 0),
                IsActive = true
            }
            ]
        };

        var service = new Service
        {
            Id = _serviceId,
            BusinessId = _businessId,
            Name = "Corte",
            Duration = 30,
            Price = 10000,
            IsActive = true
        };

        var appointment = new Appointment
        {
            StartDate = _date.ToDateTime(new TimeOnly(9, 30)),
            EndDate = _date.ToDateTime(new TimeOnly(10, 0))
        };

        _userBusinessRepository
            .GetEmployeeByIdWithDetailsAsync(_employeeId)
            .Returns(employee);

        _serviceRepository
            .GetByServiceIdAsync(_serviceId)
            .Returns(service);

        _appointmentRepository
            .GetByUserBusinessIdAndDate(_employeeId, _date)
            .Returns([appointment]);

        var result = await _appointmentService.GetEmployeeAvailabilityAsync(
            _employeeId,
            _serviceId,
            _date);

        Assert.Single(result);

        Assert.Equal(new TimeOnly(9, 0), result[0].StartTime);
        Assert.Equal(new TimeOnly(9, 30), result[0].EndTime);
    }
}