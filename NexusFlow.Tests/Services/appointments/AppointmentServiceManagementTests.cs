using NexusFlow.Exceptions;
using NexusFlow.Models.Entities;
using NexusFlow.Models.Enums;
using NSubstitute;
using Xunit;

namespace NexusFlow.Tests.Services;

public class AppointmentServiceManagementTests : AppointmentServiceTestBase
{
    [Fact]
    public async Task GetManagedAppointmentsAsync_WhenUserDoesNotBelongToBusiness_ThrowsForbiddenException()
    {
        _userBusinessRepository
            .GetByUserIdAsync(_clientId)
            .Returns((UserBusiness?)null);

        var action = () =>
            _appointmentService.GetManagedAppointmentsAsync(_clientId);

        await Assert.ThrowsAsync<ForbiddenException>(action);
    }

    [Fact]
    public async Task GetManagedAppointmentsAsync_WhenUserIsEmployee_ReturnsEmployeeAppointments()
    {
        var employee = CreateEmployee();
        employee.UserId = _clientId;
        employee.Role = BusinessRole.Employee;

        var appointment = CreateManagedAppointment();

        appointment.Client = new User
        {
            Id = _clientId,
            FirstName = "Maria",
            LastName = "Perez"
        };

        _userBusinessRepository
            .GetByUserIdAsync(_clientId)
            .Returns(employee);

        _appointmentRepository
            .GetByEmployeeIdAsync(employee.Id)
            .Returns(new List<Appointment> { appointment });

        var result = await _appointmentService.GetManagedAppointmentsAsync(_clientId);

        Assert.Single(result);

        var response = result[0];

        Assert.Equal(appointment.Id, response.Id);
        Assert.Equal("Maria Perez", response.ClientName);
        Assert.Equal("Corte", response.ServiceName);
        Assert.Equal("Juan Pérez", response.EmployeeName);
        Assert.Equal(appointment.StartDate, response.StartDate);
        Assert.Equal(appointment.EndDate, response.EndDate);
        Assert.Equal(AppointmentStatus.Pending, response.Status);
        Assert.Equal(10000, response.Price);

        await _appointmentRepository
            .Received(1)
            .GetByEmployeeIdAsync(employee.Id);
    }

    [Fact]
    public async Task GetManagedAppointmentsAsync_WhenUserIsAdmin_ReturnsBusinessAppointments()
    {
        var admin = CreateEmployee();
        admin.UserId = _clientId;
        admin.Role = BusinessRole.Admin;

        var appointment = CreateManagedAppointment();

        _userBusinessRepository
            .GetByUserIdAsync(_clientId)
            .Returns(admin);

        _appointmentRepository
            .GetByBusinessIdAsync(_businessId)
            .Returns(new List<Appointment> { appointment });

        var result = await _appointmentService.GetManagedAppointmentsAsync(_clientId);

        Assert.Single(result);

        await _appointmentRepository
            .Received(1)
            .GetByBusinessIdAsync(_businessId);
    }

    [Fact]
    public async Task GetManagedAppointmentsAsync_WhenAdminFiltersByEmployee_ReturnsEmployeeAppointments()
    {
        var admin = CreateEmployee();
        admin.UserId = _clientId;
        admin.Role = BusinessRole.Admin;

        var appointment = CreateManagedAppointment();

        _userBusinessRepository
            .GetByUserIdAsync(_clientId)
            .Returns(admin);

        _appointmentRepository
            .GetByBusinessAndEmployeeIdAsync(_businessId, _employeeId)
            .Returns(new List<Appointment> { appointment });

        var result = await _appointmentService.GetManagedAppointmentsAsync(
            _clientId,
            _employeeId);

        Assert.Single(result);

        await _appointmentRepository
            .Received(1)
            .GetByBusinessAndEmployeeIdAsync(_businessId, _employeeId);
    }

    private Appointment CreateManagedAppointment(
    AppointmentStatus status = AppointmentStatus.Pending)
    {
        var appointment = CreateAppointment(status);

        appointment.Client = new User
        {
            Id = _clientId,
            FirstName = "Cliente",
            LastName = "Test"
        };

        return appointment;
    }
}