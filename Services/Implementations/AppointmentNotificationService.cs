using NexusFlow.Repositories.Interfaces;
using NexusFlow.Services.Interfaces;

public sealed class AppointmentNotificationService : IAppointmentNotificationService
{
    private readonly IConfiguration _configuration;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<AppointmentNotificationService> _logger;
    private readonly IUserRepository _userRepository;

    public AppointmentNotificationService(
        IConfiguration configuration,
        IEmailSender emailSender,
        ILogger<AppointmentNotificationService> logger,
        IUserRepository userRepository)
    {
        _configuration = configuration;
        _emailSender = emailSender;
        _logger = logger;
        _userRepository = userRepository;
    }


    public async Task SendConfirmationAsync(Guid clientId, Guid appointmentId, string confirmationToken)
    {
        var client = await _userRepository.GetByIdAsync(clientId);
        if (client == null)
        {
            _logger.LogError("No se encontró el cliente {ClientId} para enviar la confirmación del turno {AppointmentId}", clientId, appointmentId);
            return;
        }

        var frontendUrl = _configuration["Frontend:BaseUrl"] ?? "http://localhost:5173";
        var confirmationUrl = $"{frontendUrl}/appointments/confirm?token={Uri.EscapeDataString(confirmationToken)}";

        var subject = "Confirmá tu turno - NexusFlow";

        var body = $"""
            <h2>Confirmá tu turno</h2>
            <p>Tu reserva fue creada correctamente.</p>
            <p>Tenés 1 hora para confirmar el turno.</p>
            <p>
                <a href="{confirmationUrl}">
                    Confirmar turno
                </a>
            </p>
            """;

        await _emailSender.SendAsync(client.Email, subject, body);
    }
}