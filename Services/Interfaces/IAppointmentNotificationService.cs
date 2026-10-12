public interface IAppointmentNotificationService
{
        Task SendConfirmationAsync(Guid clientId, Guid appointmentId, string confirmationToken);
}