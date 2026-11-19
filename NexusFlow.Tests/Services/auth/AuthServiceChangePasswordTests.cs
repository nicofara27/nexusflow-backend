
using NexusFlow.Exceptions;
using NexusFlow.Models.DTOs.Auth;
using NexusFlow.Models.Enums;
using NexusFlow.Models.Entities;
using NexusFlow.Repositories.Interfaces;
using NexusFlow.Services.Implementations;
using NexusFlow.Services.Interfaces;
using NSubstitute;

namespace NexusFlow.Tests.Services.Auth;

public class AuthServiceChangePasswordTests
{
    private readonly IUserRepository _userRepository =
        Substitute.For<IUserRepository>();

    private readonly IUnitOfWork _unitOfWork =
        Substitute.For<IUnitOfWork>();

    private readonly IPasswordService _passwordService =
        Substitute.For<IPasswordService>();

    private readonly AuthService _authService;
    private readonly Guid _userId = Guid.NewGuid();

    private readonly IUserBusinessRepository _userBusinessRepository =
    Substitute.For<IUserBusinessRepository>();

    private readonly IJwtService _jwtService =
        Substitute.For<IJwtService>();

    public AuthServiceChangePasswordTests()
    {
        _authService = new AuthService(
            Substitute.For<IBusinessRepository>(),
            _userRepository,
            _userBusinessRepository,
            _unitOfWork,
            _passwordService,
            _jwtService
        );
    }

    private User ConfigureUser()
    {
        var user = new User
        {
            Id = _userId,
            PasswordHash = "hash-actual",
            RefreshToken = "refresh-token-anterior",
            RefreshTokenExpiry = DateTime.UtcNow.AddDays(5)
        };

        _userRepository.GetByIdAsync(_userId).Returns(user);

        _passwordService
            .verifyPassword("Actual1234", "hash-actual")
            .Returns(true);

        return user;
    }


    [Fact]
    public async Task ChangePasswordAsync_WhenValid_UpdatesPasswordAndRenewsSession()
    {
        var user = ConfigureUser();

        _passwordService
            .hashPassword("NuevaClave123")
            .Returns("hash-nuevo");

        _jwtService
            .GenerateToken(user)
            .Returns("jwt-nuevo");

        _jwtService
            .GenerateRefreshToken()
            .Returns("refresh-token-nuevo");

        _userBusinessRepository
            .GetByUserIdAsync(_userId)
            .Returns(new UserBusiness
            {
                Role = BusinessRole.Employee
            });

        var request = new ChangePasswordRequest
        {
            CurrentPassword = "Actual1234",
            NewPassword = "NuevaClave123"
        };

        var result = await _authService.ChangePasswordAsync(
            request,
            _userId
        );

        Assert.Equal("hash-nuevo", user.PasswordHash);

        Assert.Equal("refresh-token-nuevo", user.RefreshToken);
        Assert.NotEqual("refresh-token-anterior", user.RefreshToken);
        Assert.NotNull(user.RefreshTokenExpiry);

        Assert.Equal("jwt-nuevo", result.Token);
        Assert.Equal("refresh-token-nuevo", result.RefreshToken);
        Assert.Equal(user.RefreshTokenExpiry, result.RefreshTokenExpiry);
        Assert.Equal(BusinessRole.Employee, result.BusinessRole);

        _userRepository.Received(1).Update(user);

        await _unitOfWork
            .Received(1)
            .SaveChangesAsync();
    }

    [Fact]
    public async Task ChangePasswordAsync_WhenCurrentPasswordIsWrong_ThrowsValidation()
    {
        var user = ConfigureUser();

        var request = new ChangePasswordRequest
        {
            CurrentPassword = "Incorrecta123",
            NewPassword = "NuevaClave123"
        };

        await Assert.ThrowsAsync<ValidationException>(
            () => _authService.ChangePasswordAsync(request, _userId)
        );

        Assert.Equal("hash-actual", user.PasswordHash);

        await _unitOfWork
            .DidNotReceive()
            .SaveChangesAsync();
    }

    [Theory]
    [InlineData("1234567")]
    [InlineData("        ")]
    public async Task ChangePasswordAsync_WhenNewPasswordIsInvalid_ThrowsValidation(
        string newPassword)
    {
        ConfigureUser();

        var request = new ChangePasswordRequest
        {
            CurrentPassword = "Actual1234",
            NewPassword = newPassword
        };

        await Assert.ThrowsAsync<ValidationException>(
            () => _authService.ChangePasswordAsync(request, _userId)
        );

        await _unitOfWork
            .DidNotReceive()
            .SaveChangesAsync();
    }

    [Fact]
    public async Task ChangePasswordAsync_WhenPasswordsAreEqual_ThrowsValidation()
    {
        ConfigureUser();

        var request = new ChangePasswordRequest
        {
            CurrentPassword = "Actual1234",
            NewPassword = "Actual1234"
        };

        await Assert.ThrowsAsync<ValidationException>(
            () => _authService.ChangePasswordAsync(request, _userId)
        );

        await _unitOfWork
            .DidNotReceive()
            .SaveChangesAsync();
    }

    [Fact]
    public async Task ChangePasswordAsync_WhenUserDoesNotExist_ThrowsNotFound()
    {
        _userRepository
            .GetByIdAsync(_userId)
            .Returns((User?)null);

        var request = new ChangePasswordRequest
        {
            CurrentPassword = "Actual1234",
            NewPassword = "NuevaClave123"
        };

        await Assert.ThrowsAsync<NotFoundException>(
            () => _authService.ChangePasswordAsync(request, _userId)
        );

        await _unitOfWork
            .DidNotReceive()
            .SaveChangesAsync();
    }
}
