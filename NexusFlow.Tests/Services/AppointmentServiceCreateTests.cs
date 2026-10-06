namespace NexusFlow.Tests.Services;
using NexusFlow.Exceptions;
using NexusFlow.Models.DTOs.Appointment;
using NexusFlow.Models.Entities;
using NexusFlow.Models.Enums;
using NSubstitute;
using Xunit;

public class AppointmentServiceCreateTests
    : AppointmentServiceTestBase
{
    [Fact]
    public async Task CreateAppointmentAsync_WhenScheduleHasConflict_ThrowsConflictException()
    {
        var startDate = _date.ToDateTime(new TimeOnly(9, 30));
        var endDate = startDate.AddMinutes(30);

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
                EndTime = new TimeOnly(18, 0),
                IsActive = true
            }
        ];

        var service = CreateService();

        var request = new AppointmentRequest
        {
            EmployeeId = _employeeId,
            ServiceId = _serviceId,
            StartDate = startDate
        };

        _userBusinessRepository
            .GetEmployeeByIdWithDetailsAsync(_employeeId)
            .Returns(employee);

        _serviceRepository
            .GetByIdAndBusinessAsync(_serviceId, _businessId)
            .Returns(service);

        _appointmentRepository
            .HasConflictAsync(_employeeId, startDate, endDate)
            .Returns(true);

        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => _appointmentService.CreateAppointmentAsync(
                _clientId,
                request));

        Assert.Equal(
            "El empleado ya tiene un turno en ese horario.",
            exception.Message);

        _appointmentRepository
            .DidNotReceive()
            .Add(Arg.Any<Appointment>());

        await _unitOfWork
            .DidNotReceive()
            .SaveChangesAsync();
    }

    [Fact]
    public async Task CreateAppointmentAsync_WhenDataIsValid_CreatesAppointment()
    {
        var startDate = _date.ToDateTime(new TimeOnly(9, 30));
        var endDate = startDate.AddMinutes(30);

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
                EndTime = new TimeOnly(18, 0),
                IsActive = true
            }
        ];

        var service = CreateService();

        var request = new AppointmentRequest
        {
            EmployeeId = _employeeId,
            ServiceId = _serviceId,
            StartDate = startDate
        };

        Appointment? createdAppointment = null;

        _userBusinessRepository
            .GetEmployeeByIdWithDetailsAsync(_employeeId)
            .Returns(employee);

        _serviceRepository
            .GetByIdAndBusinessAsync(_serviceId, _businessId)
            .Returns(service);

        _appointmentRepository
            .HasConflictAsync(_employeeId, startDate, endDate)
            .Returns(false);

        _appointmentRepository
            .When(x => x.Add(Arg.Any<Appointment>()))
            .Do(call =>
            {
                createdAppointment = call.Arg<Appointment>();
            });

        var result = await _appointmentService.CreateAppointmentAsync(
            _clientId,
            request);

        Assert.NotNull(createdAppointment);

        Assert.Equal(_clientId, createdAppointment.ClientId);
        Assert.Equal(_employeeId, createdAppointment.EmployeeId);
        Assert.Equal(_serviceId, createdAppointment.ServiceId);
        Assert.Equal(_businessId, createdAppointment.BusinessId);
        Assert.Equal(startDate, createdAppointment.StartDate);
        Assert.Equal(endDate, createdAppointment.EndDate);
        Assert.Equal(AppointmentStatus.Pending, createdAppointment.Status);

        Assert.Equal("Corte", result.ServiceName);
        Assert.Equal("Juan Pérez", result.EmployeeName);
        Assert.Equal(startDate, result.StartDate);
        Assert.Equal(endDate, result.EndDate);
        Assert.Equal(AppointmentStatus.Pending, result.Status);
        Assert.Equal(10000, result.Price);

        _appointmentRepository
            .Received(1)
            .Add(Arg.Any<Appointment>());

        await _unitOfWork
            .Received(1)
            .SaveChangesAsync();
    }

    [Fact]
    public async Task CreateAppointmentAsync_WhenEmployeeDoesNotWorkThatDay_ThrowsBusinessRuleException()
    {
        var startDate = _date.ToDateTime(new TimeOnly(9, 30));

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
            EndTime = new TimeOnly(18, 0),
            IsActive = true
        }
        ];

        var service = CreateService();

        var request = new AppointmentRequest
        {
            EmployeeId = _employeeId,
            ServiceId = _serviceId,
            StartDate = startDate
        };

        _userBusinessRepository
            .GetEmployeeByIdWithDetailsAsync(_employeeId)
            .Returns(employee);

        _serviceRepository
            .GetByIdAndBusinessAsync(_serviceId, _businessId)
            .Returns(service);

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(
            () => _appointmentService.CreateAppointmentAsync(
                _clientId,
                request));

        Assert.Equal(
            "El empleado no trabaja ese día.",
            exception.Message);

        _appointmentRepository
            .DidNotReceive()
            .Add(Arg.Any<Appointment>());

        await _unitOfWork
            .DidNotReceive()
            .SaveChangesAsync();
    }

    [Fact]
    public async Task CreateAppointmentAsync_WhenEmployeeDoesNotPerformService_ThrowsBusinessRuleException()
    {
        var startDate = _date.ToDateTime(new TimeOnly(9, 30));

        var employee = CreateEmployee();
        employee.ServiceAssignment = [];

        var service = CreateService();

        var request = new AppointmentRequest
        {
            EmployeeId = _employeeId,
            ServiceId = _serviceId,
            StartDate = startDate
        };

        _userBusinessRepository
            .GetEmployeeByIdWithDetailsAsync(_employeeId)
            .Returns(employee);

        _serviceRepository
            .GetByIdAndBusinessAsync(_serviceId, _businessId)
            .Returns(service);

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(
            () => _appointmentService.CreateAppointmentAsync(
                _clientId,
                request));

        Assert.Equal(
            "El empleado no realiza este servicio.",
            exception.Message);

        await _appointmentRepository
            .DidNotReceive()
            .HasConflictAsync(
                Arg.Any<Guid>(),
                Arg.Any<DateTime>(),
                Arg.Any<DateTime>());

        _appointmentRepository
            .DidNotReceive()
            .Add(Arg.Any<Appointment>());

        await _unitOfWork
            .DidNotReceive()
            .SaveChangesAsync();
    }

    [Fact]
    public async Task CreateAppointmentAsync_WhenEmployeeDoesNotExist_ThrowsNotFoundException()
    {
        var request = new AppointmentRequest
        {
            EmployeeId = _employeeId,
            ServiceId = _serviceId,
            StartDate = _date.ToDateTime(new TimeOnly(9, 30))
        };

        _userBusinessRepository
            .GetEmployeeByIdWithDetailsAsync(_employeeId)
            .Returns((UserBusiness?)null);

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _appointmentService.CreateAppointmentAsync(
                _clientId,
                request));

        Assert.Equal("Empleado no encontrado.", exception.Message);

        _appointmentRepository
            .DidNotReceive()
            .Add(Arg.Any<Appointment>());

        await _unitOfWork
            .DidNotReceive()
            .SaveChangesAsync();
    }

    [Fact]
    public async Task CreateAppointmentAsync_WhenEmployeeIsInactive_ThrowsNotFoundException()
    {
        var employee = CreateEmployee(isActive: false);

        var request = new AppointmentRequest
        {
            EmployeeId = _employeeId,
            ServiceId = _serviceId,
            StartDate = _date.ToDateTime(new TimeOnly(9, 30))
        };

        _userBusinessRepository
            .GetEmployeeByIdWithDetailsAsync(_employeeId)
            .Returns(employee);

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _appointmentService.CreateAppointmentAsync(
                _clientId,
                request));

        Assert.Equal("Empleado no encontrado.", exception.Message);

        _appointmentRepository
            .DidNotReceive()
            .Add(Arg.Any<Appointment>());

        await _unitOfWork
            .DidNotReceive()
            .SaveChangesAsync();
    }

    [Fact]
    public async Task CreateAppointmentAsync_WhenServiceDoesNotExist_ThrowsNotFoundException()
    {
        var employee = CreateEmployee();

        var request = new AppointmentRequest
        {
            EmployeeId = _employeeId,
            ServiceId = _serviceId,
            StartDate = _date.ToDateTime(new TimeOnly(9, 30))
        };

        _userBusinessRepository
            .GetEmployeeByIdWithDetailsAsync(_employeeId)
            .Returns(employee);

        _serviceRepository
            .GetByIdAndBusinessAsync(_serviceId, _businessId)
            .Returns((Service?)null);

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _appointmentService.CreateAppointmentAsync(
                _clientId,
                request));

        Assert.Equal("Servicio no encontrado.", exception.Message);

        _appointmentRepository
            .DidNotReceive()
            .Add(Arg.Any<Appointment>());

        await _unitOfWork
            .DidNotReceive()
            .SaveChangesAsync();
    }

    [Fact]
    public async Task CreateAppointmentAsync_WhenServiceIsInactive_ThrowsNotFoundException()
    {
        var employee = CreateEmployee();
        var service = CreateService(isActive: false);

        var request = new AppointmentRequest
        {
            EmployeeId = _employeeId,
            ServiceId = _serviceId,
            StartDate = _date.ToDateTime(new TimeOnly(9, 30))
        };

        _userBusinessRepository
            .GetEmployeeByIdWithDetailsAsync(_employeeId)
            .Returns(employee);

        _serviceRepository
            .GetByIdAndBusinessAsync(_serviceId, _businessId)
            .Returns(service);

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _appointmentService.CreateAppointmentAsync(
                _clientId,
                request));

        Assert.Equal("Servicio no encontrado.", exception.Message);

        _appointmentRepository
            .DidNotReceive()
            .Add(Arg.Any<Appointment>());

        await _unitOfWork
            .DidNotReceive()
            .SaveChangesAsync();
    }

    [Fact]
    public async Task CreateAppointmentAsync_WhenEmployeeScheduleIsInactive_ThrowsBusinessRuleException()
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
            EndTime = new TimeOnly(18, 0),
            IsActive = false
        }
        ];

        var service = CreateService();

        var request = new AppointmentRequest
        {
            EmployeeId = _employeeId,
            ServiceId = _serviceId,
            StartDate = _date.ToDateTime(new TimeOnly(9, 30))
        };

        _userBusinessRepository
            .GetEmployeeByIdWithDetailsAsync(_employeeId)
            .Returns(employee);

        _serviceRepository
            .GetByIdAndBusinessAsync(_serviceId, _businessId)
            .Returns(service);

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(
            () => _appointmentService.CreateAppointmentAsync(
                _clientId,
                request));

        Assert.Equal(
            "El empleado no trabaja ese día.",
            exception.Message);

        _appointmentRepository
            .DidNotReceive()
            .Add(Arg.Any<Appointment>());

        await _unitOfWork
            .DidNotReceive()
            .SaveChangesAsync();
    }

    [Theory]
    [InlineData(8, 45)]
    [InlineData(17, 45)]
    public async Task CreateAppointmentAsync_WhenAppointmentIsOutsideWorkSchedule_ThrowsBusinessRuleException(
    int hour,
    int minute)
    {
        var startDate = _date.ToDateTime(new TimeOnly(hour, minute));

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
            EndTime = new TimeOnly(18, 0),
            IsActive = true
        }
        ];

        var service = CreateService();

        var request = new AppointmentRequest
        {
            EmployeeId = _employeeId,
            ServiceId = _serviceId,
            StartDate = startDate
        };

        _userBusinessRepository
            .GetEmployeeByIdWithDetailsAsync(_employeeId)
            .Returns(employee);

        _serviceRepository
            .GetByIdAndBusinessAsync(_serviceId, _businessId)
            .Returns(service);

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(
            () => _appointmentService.CreateAppointmentAsync(
                _clientId,
                request));

        Assert.Equal(
            "El horario está fuera del horario laboral del empleado.",
            exception.Message);

        _appointmentRepository
            .DidNotReceive()
            .Add(Arg.Any<Appointment>());

        await _unitOfWork
            .DidNotReceive()
            .SaveChangesAsync();
    }
}
