public sealed class AppointmentNotificationService(
    ILogger<AppointmentNotificationService> logger,
    IConfiguration configuration) : IAppointmentNotificationService
{
    public Task SendConfirmationAsync(Guid clientId, Guid appointmentId, string confirmationToken)
    {
        var frontendUrl = configuration["Frontend:BaseUrl"] ?? "http://localhost:5173";
        var confirmationUrl = $"{frontendUrl}/appointments/confirm?token={Uri.EscapeDataString(confirmationToken)}";

        logger.LogInformation(
            "Enlace de confirmación para el turno {AppointmentId}: {ConfirmationUrl}",
            appointmentId,
            confirmationUrl);

        return Task.CompletedTask;
    }
}