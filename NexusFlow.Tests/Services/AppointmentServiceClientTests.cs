using NexusFlow.Models.Entities;
using NexusFlow.Models.Enums;
using NSubstitute;

namespace NexusFlow.Tests.Services;

public class AppointmentServiceClientAppointmentsTests : AppointmentServiceTestBase
{
    [Fact]
    public async Task GetClientAppointmentsAsync_WhenAppointmentsExist_ReturnsMappedAppointments()
    {
        var appointment = CreateAppointment(AppointmentStatus.Confirmed);

        appointment.Business = new Business
        {
            Id = _businessId,
            Name = "Barbería Centro"
        };

        var appointments = new List<Appointment>
        {
            appointment
        };

        _appointmentRepository
            .GetByClientIdAsync(_clientId)
            .Returns(Task.FromResult(appointments));

        var result = await _appointmentService.GetClientAppointmentsAsync(_clientId);

        Assert.Single(result);

        var response = result[0];

        Assert.Equal(appointment.Id, response.Id);
        Assert.Equal(_businessId, response.BusinessId);
        Assert.Equal("Barbería Centro", response.BusinessName);
        Assert.Equal("Corte", response.ServiceName);
        Assert.Equal("Juan Pérez", response.EmployeeName);
        Assert.Equal(appointment.StartDate, response.StartDate);
        Assert.Equal(appointment.EndDate, response.EndDate);
        Assert.Equal(AppointmentStatus.Confirmed, response.Status);
        Assert.Equal(10000, response.Price);

        await _appointmentRepository
            .Received(1)
            .GetByClientIdAsync(_clientId);
    }

    [Fact]
    public async Task GetClientAppointmentsAsync_WhenClientHasNoAppointments_ReturnsEmptyList()
    {
        _appointmentRepository
            .GetByClientIdAsync(_clientId)
            .Returns(Task.FromResult(new List<Appointment>()));

        var result = await _appointmentService.GetClientAppointmentsAsync(_clientId);

        Assert.Empty(result);

        await _appointmentRepository
            .Received(1)
            .GetByClientIdAsync(_clientId);
    }
}