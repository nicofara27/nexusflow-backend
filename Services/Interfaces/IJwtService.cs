using NexusFlow.Models.Entities;

namespace NexusFlow.Services.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(User user);
        string GenerateRefreshToken();

    }
}
