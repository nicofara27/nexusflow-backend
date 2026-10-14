using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NexusFlow.Models.Entities;
using NexusFlow.Repositories.Interfaces;
using NexusFlow.Services.Implementations;
using NexusFlow.Services.Interfaces;
using NSubstitute;
using Xunit;

namespace NexusFlow.Tests.Services;

public class AppointmentNotificationServiceTests
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<AppointmentNotificationService> _logger;
    private readonly AppointmentNotificationService _notificationService;

    private readonly Guid _clientId = Guid.NewGuid();
    private readonly Guid _appointmentId = Guid.NewGuid();

    public AppointmentNotificationServiceTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _emailSender = Substitute.For<IEmailSender>();
        _logger = Substitute.For<ILogger<AppointmentNotificationService>>();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Frontend:BaseUrl"] = "http://localhost:5173"
            })
            .Build();

        _notificationService = new AppointmentNotificationService(
            configuration,
            _emailSender,
            _logger,
            _userRepository);
    }

    [Fact]
    public async Task SendConfirmationAsync_WhenClientExists_SendsEmail()
    {
        var token = "confirmation-token";

        var client = new User
        {
            Id = _clientId,
            Email = "cliente@test.com"
        };

        _userRepository
            .GetByIdAsync(_clientId)
            .Returns(client);

        await _notificationService.SendConfirmationAsync(_clientId, _appointmentId, token);

        await _emailSender
            .Received(1)
            .SendAsync(
                "cliente@test.com",
                Arg.Any<string>(),
                Arg.Is<string>(body =>
                    body.Contains("http://localhost:5173") &&
                    body.Contains(token)));
    }

    [Fact]
    public async Task SendConfirmationAsync_WhenClientDoesNotExist_DoesNotSendEmail()
    {
        _userRepository
            .GetByIdAsync(_clientId)
            .Returns((User?)null);

        await _notificationService.SendConfirmationAsync(
            _clientId,
            _appointmentId,
            "confirmation-token");

        await _emailSender
            .DidNotReceive()
            .SendAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>());
    }
}