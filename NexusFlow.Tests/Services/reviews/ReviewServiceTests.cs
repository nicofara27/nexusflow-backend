using NexusFlow.Exceptions;
using NexusFlow.Models.Entities;
using NexusFlow.Models.Enums;
using NexusFlow.Repositories.Interfaces;
using NexusFlow.Services.Implementations;
using NSubstitute;

namespace NexusFlow.Tests.Services;

public class ReviewServiceTests
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IReviewRepository _reviewRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ReviewService _reviewService;

    private readonly Guid _clientId = Guid.NewGuid();
    private readonly Guid _appointmentId = Guid.NewGuid();
    private readonly Guid _businessId = Guid.NewGuid();

    public ReviewServiceTests()
    {
        _appointmentRepository = Substitute.For<IAppointmentRepository>();
        _reviewRepository = Substitute.For<IReviewRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();

        _reviewService = new ReviewService(
            _appointmentRepository,
            _reviewRepository,
            _unitOfWork
        );
    }

    private Appointment CreateAppointment(
        AppointmentStatus status = AppointmentStatus.Completed)
    {
        return new Appointment
        {
            Id = _appointmentId,
            ClientId = _clientId,
            BusinessId = _businessId,
            Status = status,
            Client = new User
            {
                FirstName = "Juan",
                LastName = "Pérez"
            }
        };
    }

    private ReviewRequest CreateRequest(int rating = 5)
    {
        return new ReviewRequest
        {
            AppointmentId = _appointmentId,
            Rating = rating,
            Comment = "Muy buen servicio"
        };
    }

    [Fact]
    public async Task CreateAsync_WhenRequestIsValid_CreatesReview()
    {
        var appointment = CreateAppointment();
        var request = CreateRequest();

        _appointmentRepository
            .GetWithDetailsByIdAsync(_appointmentId)
            .Returns(appointment);

        var result = await _reviewService.CreateAsync(_clientId, request);

        Assert.Equal(5, result.Rating);
        Assert.Equal("Muy buen servicio", result.Comment);
        Assert.Equal("Juan Pérez", result.Author);

        await _reviewRepository
            .Received(1)
            .AddAsync(Arg.Is<Review>(r =>
                r.AppointmentId == _appointmentId &&
                r.BusinessId == _businessId &&
                r.Rating == 5));

        await _unitOfWork
            .Received(1)
            .SaveChangesAsync();
    }

    [Fact]
    public async Task CreateAsync_WhenUserIsNotClient_ThrowsForbiddenException()
    {
        var appointment = CreateAppointment();
        var request = CreateRequest();

        _appointmentRepository
            .GetWithDetailsByIdAsync(_appointmentId)
            .Returns(appointment);

        var otherUserId = Guid.NewGuid();

        await Assert.ThrowsAsync<ForbiddenException>(
            () => _reviewService.CreateAsync(otherUserId, request));

        await _reviewRepository
            .DidNotReceive()
            .AddAsync(Arg.Any<Review>());
    }

    [Theory]
    [InlineData(AppointmentStatus.Pending)]
    [InlineData(AppointmentStatus.Confirmed)]
    [InlineData(AppointmentStatus.Cancelled)]
    public async Task CreateAsync_WhenAppointmentIsNotCompleted_ThrowsBusinessRuleException(
        AppointmentStatus status)
    {
        var appointment = CreateAppointment(status);
        var request = CreateRequest();

        _appointmentRepository
            .GetWithDetailsByIdAsync(_appointmentId)
            .Returns(appointment);

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => _reviewService.CreateAsync(_clientId, request));

        await _reviewRepository
            .DidNotReceive()
            .AddAsync(Arg.Any<Review>());
    }

    [Fact]
    public async Task CreateAsync_WhenReviewAlreadyExists_ThrowsConflictException()
    {
        var appointment = CreateAppointment();
        var request = CreateRequest();

        _appointmentRepository
            .GetWithDetailsByIdAsync(_appointmentId)
            .Returns(appointment);

        _reviewRepository
            .GetByAppointmentIdAsync(_appointmentId)
            .Returns(new Review
            {
                AppointmentId = _appointmentId,
                BusinessId = _businessId,
                Rating = 4
            });

        await Assert.ThrowsAsync<ConflictException>(
            () => _reviewService.CreateAsync(_clientId, request));

        await _reviewRepository
            .DidNotReceive()
            .AddAsync(Arg.Any<Review>());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public async Task CreateAsync_WhenRatingIsInvalid_ThrowsValidationException(
        int rating)
    {
        var request = CreateRequest(rating);

        await Assert.ThrowsAsync<NexusFlow.Exceptions.ValidationException>(
            () => _reviewService.CreateAsync(_clientId, request));

        await _reviewRepository
            .DidNotReceive()
            .AddAsync(Arg.Any<Review>());
    }
}
