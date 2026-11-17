namespace NexusFlow.Tests.Services;
using NexusFlow.Exceptions;
using NexusFlow.Models.Entities;
using NSubstitute;
using Xunit;

public class AppointmentServiceAvailabilityTests
    : AppointmentServiceTestBase
{
    [Fact]
    public async Task GetEmployeeAvailabilityAsync_WhenEmployeeDoesNotWorkThatDay_ReturnsEmptyList()
    {
        var employee = CreateEmployee();

        employee.ServiceAssignment =
        [
            new ServiceAssignment
            {
                ServiceId = _serviceId
            }
        ];

        employee.Schedules =
        [
            new EmployeeSchedule
            {
                DayOfWeek = DayOfWeek.Tuesday,
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(10, 0),
                IsActive = true
            }
        ];

        var service = CreateService();

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
        var employee = CreateEmployee(isActive: false);

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
        var employee = CreateEmployee();

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
        var employee = CreateEmployee();

        var service = CreateService(isActive: false);

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
        var employee = CreateEmployee();

        var service = CreateService();

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
        var employee = CreateEmployee();

        employee.ServiceAssignment =
        [
            new ServiceAssignment
            {
                ServiceId = _serviceId
            }
        ];

        employee.Schedules =
        [
            new EmployeeSchedule
            {
                DayOfWeek = _date.DayOfWeek,
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(10, 0),
                IsActive = true
            }
        ];

        var service = CreateService();

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
        var employee = CreateEmployee();

        employee.ServiceAssignment =
        [
            new ServiceAssignment
            {
                ServiceId = _serviceId
            }
        ];
        employee.Schedules =
        [
            new EmployeeSchedule
            {
                DayOfWeek = _date.DayOfWeek,
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(10, 0),
                IsActive = true
            }
        ];

        var service = CreateService();

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