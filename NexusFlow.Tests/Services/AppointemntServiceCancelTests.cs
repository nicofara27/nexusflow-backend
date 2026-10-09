using NexusFlow.Exceptions;
using NexusFlow.Models.Entities;
using NexusFlow.Models.Enums;
using NSubstitute;
using Xunit;

namespace NexusFlow.Tests.Services;

public class AppointmentServiceCancelTests : AppointmentServiceTestBase
{
    [Theory]
    [InlineData(AppointmentStatus.Pending)]
    [InlineData(AppointmentStatus.Confirmed)]
    public async Task CancelAppointmentAsync_WhenStatusCanBeCancelled_CancelsAppointment(
        AppointmentStatus status)
    {
        var appointment = CreateAppointment(status: status);

        _appointmentRepository
            .GetWithDetailsByIdAsync(_appointmentId)
            .Returns(appointment);

        var result = await _appointmentService.CancelAppointmentAsync(_clientId, _appointmentId);

        Assert.Equal(AppointmentStatus.Cancelled, appointment.Status);
        Assert.Equal(AppointmentStatus.Cancelled, result.Status);

        _appointmentRepository
            .Received(1)
            .UpdateStatus(appointment);

        await _unitOfWork
            .Received(1)
            .SaveChangesAsync();
    }

    [Fact]
    public async Task CancelAppointmentAsync_WhenUserHasNoPermission_ThrowsForbiddenException()
    {
        var unauthorizedUserId = Guid.NewGuid();
        var appointment = CreateAppointment();

        _appointmentRepository
            .GetWithDetailsByIdAsync(_appointmentId)
            .Returns(appointment);

        var exception = await Assert.ThrowsAsync<ForbiddenException>(
            () => _appointmentService.CancelAppointmentAsync(unauthorizedUserId, _appointmentId));

        Assert.Equal("No tenés permiso para cancelar este turno.", exception.Message);

        _appointmentRepository
            .DidNotReceive()
            .UpdateStatus(Arg.Any<Appointment>());

        await _unitOfWork
            .DidNotReceive()
            .SaveChangesAsync();
    }

    [Fact]
    public async Task CancelAppointmentAsync_WhenUserIsEmployee_CancelsAppointment()
    {
        var appointment = CreateAppointment(employeeUserId: _employeeUserId);

        _appointmentRepository
            .GetWithDetailsByIdAsync(_appointmentId)
            .Returns(appointment);

        _userBusinessRepository
            .IsAdminAsync(_employeeUserId, _businessId)
            .Returns(false);

        var result = await _appointmentService.CancelAppointmentAsync(_employeeUserId, _appointmentId);

        Assert.Equal(AppointmentStatus.Cancelled, appointment.Status);
        Assert.Equal(AppointmentStatus.Cancelled, result.Status);

        _appointmentRepository
            .Received(1)
            .UpdateStatus(appointment);

        await _unitOfWork
            .Received(1)
            .SaveChangesAsync();
    }

    [Fact]
    public async Task CancelAppointmentAsync_WhenUserIsAdmin_CancelsAppointment()
    {
        var adminUserId = Guid.NewGuid();
        var appointment = CreateAppointment();

        _appointmentRepository
            .GetWithDetailsByIdAsync(_appointmentId)
            .Returns(appointment);

        _userBusinessRepository
            .IsAdminAsync(adminUserId, _businessId)
            .Returns(true);

        var result = await _appointmentService.CancelAppointmentAsync(adminUserId, _appointmentId);

        Assert.Equal(AppointmentStatus.Cancelled, appointment.Status);
        Assert.Equal(AppointmentStatus.Cancelled, result.Status);

        _appointmentRepository
            .Received(1)
            .UpdateStatus(appointment);

        await _unitOfWork
            .Received(1)
            .SaveChangesAsync();
    }

    [Theory]
    [InlineData(AppointmentStatus.Completed)]
    [InlineData(AppointmentStatus.Cancelled)]
    public async Task CancelAppointmentAsync_WhenStatusCannotBeCancelled_ThrowsBusinessRuleException(
    AppointmentStatus status)
    {
        var appointment = CreateAppointment(status: status);

        _appointmentRepository
            .GetWithDetailsByIdAsync(_appointmentId)
            .Returns(appointment);

        _userBusinessRepository
            .IsAdminAsync(_clientId, _businessId)
            .Returns(false);

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(
            () => _appointmentService.CancelAppointmentAsync(_clientId, _appointmentId));

        Assert.Equal($"No se puede cancelar un turno en estado {status}.", exception.Message);

        _appointmentRepository
            .DidNotReceive()
            .UpdateStatus(Arg.Any<Appointment>());

        await _unitOfWork
            .DidNotReceive()
            .SaveChangesAsync();
    }

    [Fact]
    public async Task CancelAppointmentAsync_WhenAppointmentDoesNotExist_ThrowsNotFoundException()
    {
        _appointmentRepository
            .GetWithDetailsByIdAsync(_appointmentId)
            .Returns((Appointment?)null);

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _appointmentService.CancelAppointmentAsync(_clientId, _appointmentId));

        Assert.Equal("Turno no encontrado.", exception.Message);

        _appointmentRepository
            .DidNotReceive()
            .UpdateStatus(Arg.Any<Appointment>());

        await _unitOfWork
            .DidNotReceive()
            .SaveChangesAsync();
    }
}