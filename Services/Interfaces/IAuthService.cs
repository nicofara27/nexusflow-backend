using NexusFlow.Models.DTOs.Auth;

namespace NexusFlow.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResult> RegisterAsync(RegisterRequest dto);
        Task<AuthResult> LoginAsync(LoginRequest dto);
        Task<RegisterResponse> RegisterEmployeeAsync(RegisterRequest dto, Guid adminId);
        Task<RegisterResponse> RegisterAdminAsync(RegisterAdminRequest dto, Guid superAdminId);
        Task ChangePasswordAsync(ChangePasswordRequest dto, Guid userId);
        Task<AuthResult> RefreshAsync(string refreshToken);
        Task LogoutAsync(string refreshToken);
    }
}
