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

        var appointment = CreateAppointment();

        _userBusinessRepository
            .GetByUserIdAsync(_clientId)
            .Returns(employee);

        _appointmentRepository
            .GetByEmployeeIdAsync(employee.Id)
            .Returns(new List<Appointment> { appointment });

        var result = await _appointmentService.GetManagedAppointmentsAsync(_clientId);

        Assert.Single(result);

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

        var appointment = CreateAppointment();

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

        var appointment = CreateAppointment();

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
}