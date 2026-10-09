using NexusFlow.Exceptions;
using NexusFlow.Models.Entities;
using NexusFlow.Models.Enums;
using NSubstitute;
using Xunit;

namespace NexusFlow.Tests.Services;

public class AppointmentServiceCompleteTests : AppointmentServiceTestBase
{
    [Fact]
    public async Task CompleteAppointmentAsync_WhenUserIsEmployee_CompletesAppointment()
    {
        var appointment = CreateAppointment(
            status: AppointmentStatus.Confirmed,
            employeeUserId: _employeeUserId);

        _appointmentRepository
            .GetWithDetailsByIdAsync(_appointmentId)
            .Returns(appointment);

        var result = await _appointmentService.CompleteAppointmentAsync(_employeeUserId, _appointmentId);

        Assert.Equal(AppointmentStatus.Completed, appointment.Status);
        Assert.Equal(AppointmentStatus.Completed, result.Status);

        _appointmentRepository
            .Received(1)
            .UpdateStatus(appointment);

        await _unitOfWork
            .Received(1)
            .SaveChangesAsync();
    }

    [Fact]
    public async Task CompleteAppointmentAsync_WhenUserIsAdmin_CompletesAppointment()
    {
        var adminUserId = Guid.NewGuid();
        var appointment = CreateAppointment(status: AppointmentStatus.Confirmed);

        _appointmentRepository
            .GetWithDetailsByIdAsync(_appointmentId)
            .Returns(appointment);

        _userBusinessRepository
            .IsAdminAsync(adminUserId, _businessId)
            .Returns(true);

        var result = await _appointmentService.CompleteAppointmentAsync(adminUserId, _appointmentId);

        Assert.Equal(AppointmentStatus.Completed, appointment.Status);
        Assert.Equal(AppointmentStatus.Completed, result.Status);

        _appointmentRepository
            .Received(1)
            .UpdateStatus(appointment);

        await _unitOfWork
            .Received(1)
            .SaveChangesAsync();
    }

    [Fact]
    public async Task CompleteAppointmentAsync_WhenAppointmentDoesNotExist_ThrowsNotFoundException()
    {
        _appointmentRepository
            .GetWithDetailsByIdAsync(_appointmentId)
            .Returns((Appointment?)null);

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _appointmentService.CompleteAppointmentAsync(_clientId, _appointmentId));

        Assert.Equal("Turno no encontrado.", exception.Message);

        _appointmentRepository
            .DidNotReceive()
            .UpdateStatus(Arg.Any<Appointment>());

        await _unitOfWork
            .DidNotReceive()
            .SaveChangesAsync();
    }

    [Fact]
    public async Task CompleteAppointmentAsync_WhenUserIsClient_ThrowsForbiddenException()
    {
        var appointment = CreateAppointment(status: AppointmentStatus.Confirmed);

        _appointmentRepository
            .GetWithDetailsByIdAsync(_appointmentId)
            .Returns(appointment);

        var exception = await Assert.ThrowsAsync<ForbiddenException>(
            () => _appointmentService.CompleteAppointmentAsync(_clientId, _appointmentId));

        Assert.Equal("No tenés permiso para completar este turno.", exception.Message);

        _appointmentRepository
            .DidNotReceive()
            .UpdateStatus(Arg.Any<Appointment>());

        await _unitOfWork
            .DidNotReceive()
            .SaveChangesAsync();
    }

    [Theory]
    [InlineData(AppointmentStatus.Pending)]
    [InlineData(AppointmentStatus.Cancelled)]
    [InlineData(AppointmentStatus.Completed)]
    public async Task CompleteAppointmentAsync_WhenAppointmentIsNotConfirmed_ThrowsBusinessRuleException(
    AppointmentStatus status)
    {
        var appointment = CreateAppointment(
            status: status,
            employeeUserId: _employeeUserId);

        _appointmentRepository
            .GetWithDetailsByIdAsync(_appointmentId)
            .Returns(appointment);

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(
            () => _appointmentService.CompleteAppointmentAsync(_employeeUserId, _appointmentId));

        Assert.Equal("Solo se puede completar un turno confirmado.", exception.Message);

        _appointmentRepository
            .DidNotReceive()
            .UpdateStatus(Arg.Any<Appointment>());

        await _unitOfWork
            .DidNotReceive()
            .SaveChangesAsync();
    }
}