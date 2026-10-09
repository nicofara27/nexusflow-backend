using NexusFlow.Exceptions;
using NexusFlow.Models.Entities;
using NexusFlow.Models.Enums;
using NexusFlow.Services.Helpers;
using NSubstitute;
using Xunit;

namespace NexusFlow.Tests.Services;

public class AppointmentServiceConfirmationTests : AppointmentServiceTestBase
{
	[Fact]
	public async Task ConfirmAppointmentAsync_WhenTokenDoesNotExist_ThrowsNotFoundException()
	{
		var token = "invalid-token";
		var tokenHash = ConfirmationTokenHelper.Hash(token);

		_appointmentRepository
			.GetByConfirmationTokenHashAsync(tokenHash)
			.Returns((Appointment?)null);

		var exception = await Assert.ThrowsAsync<NotFoundException>(
			() => _appointmentService.ConfirmAppointmentAsync(token));

		Assert.Equal("Token de confirmación inválido.", exception.Message);

		_appointmentRepository
			.DidNotReceive()
			.UpdateStatus(Arg.Any<Appointment>());

		await _unitOfWork
			.DidNotReceive()
			.SaveChangesAsync();
	}

	[Fact]
	public async Task ConfirmAppointmentAsync_WhenTokenIsExpired_ThrowsBusinessRuleException()
	{
		var token = "expired-token";
		var tokenHash = ConfirmationTokenHelper.Hash(token);

		var appointment = CreateAppointment(
			confirmationTokenHash: tokenHash,
			confirmationTokenExpiresAt: DateTime.UtcNow.AddMinutes(-10));

		_appointmentRepository.GetByConfirmationTokenHashAsync(tokenHash)
			.Returns(appointment);

		var exception = await Assert.ThrowsAsync<BusinessRuleException>(
			() => _appointmentService.ConfirmAppointmentAsync(token));

		Assert.Equal("El token de confirmación expiró.", exception.Message);

		_appointmentRepository
			.DidNotReceive()
			.UpdateStatus(Arg.Any<Appointment>());

		await _unitOfWork
			.DidNotReceive()
			.SaveChangesAsync();
	}

	[Fact]
	public async Task ConfirmAppointmentAsync_WhenTokenIsValid_ConfirmsAppointment()
	{
		var token = "valid-token";
		var tokenHash = ConfirmationTokenHelper.Hash(token);

		var appointment = CreateAppointment(
			confirmationTokenHash: tokenHash,
			confirmationTokenExpiresAt: DateTime.UtcNow.AddMinutes(10));

		_appointmentRepository
			.GetByConfirmationTokenHashAsync(tokenHash)
			.Returns(appointment);

		var result = await _appointmentService.ConfirmAppointmentAsync(token);

		Assert.Equal(AppointmentStatus.Confirmed, appointment.Status);
		Assert.Equal(AppointmentStatus.Confirmed, result.Status);

		Assert.Null(appointment.ConfirmationTokenHash);
		Assert.Null(appointment.ConfirmationTokenExpiresAt);

		_appointmentRepository
			.Received(1)
			.UpdateStatus(appointment);

		await _unitOfWork
			.Received(1)
			.SaveChangesAsync();
	}

	[Theory]
	[InlineData(AppointmentStatus.Confirmed)]
	[InlineData(AppointmentStatus.Cancelled)]
	[InlineData(AppointmentStatus.Completed)]
	public async Task ConfirmAppointmentAsync_WhenAppointmentIsNotPending_ThrowsBusinessRuleException(AppointmentStatus status)
	{
		var token = "valid-token";
		var hashToken = ConfirmationTokenHelper.Hash(token);

		var appointment = CreateAppointment(
			status: status,
			confirmationTokenHash: hashToken,
			confirmationTokenExpiresAt: DateTime.UtcNow.AddMinutes(10));

		_appointmentRepository
			.GetByConfirmationTokenHashAsync(hashToken)
			.Returns(appointment);

		var exception = await Assert.ThrowsAsync<BusinessRuleException>(
			() => _appointmentService.ConfirmAppointmentAsync(token));

		Assert.Equal("El turno ya no está pendiente de confirmación.", exception.Message);

		_appointmentRepository
			.DidNotReceive()
			.UpdateStatus(Arg.Any<Appointment>());

		await _unitOfWork
			.DidNotReceive()
			.SaveChangesAsync();
	}
}