using NexusFlow.Models.Entities;

namespace NexusFlow.Repositories.Interfaces
{
    public interface IUserRepository
    {
        void Add(User user);
        void Update(User user);
        Task<bool> ExistsByEmailAsync(string email);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByRefreshTokenAsync(string refreshToken);
        Task<bool> IsSuperAdminAsync(Guid id);
    }
}
