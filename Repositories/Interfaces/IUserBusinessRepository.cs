using NexusFlow.Models.Entities;
using NexusFlow.Models.Enums;

namespace NexusFlow.Repositories.Interfaces
{
    public interface IUserBusinessRepository
    {
        void Add(UserBusiness userBusiness);
        Task<bool> IsAdminAsync(Guid userId, Guid businessId);
        Task<bool> IsAdminOrEmployeeAsync(Guid userId, Guid businessId);
        Task<UserBusiness?> GetEmployeeByIdAsync(Guid employeeId);
        Task<UserBusiness?> GetEmployeeByIdWithDetailsAsync(Guid employeeId);
        Task<UserBusiness?> GetByUserIdAsync(Guid adminId, BusinessRole? role = null);
        Task<UserBusiness?> GetByUserIdWithBusinessAsync(Guid userId);
        Task<Business?> GetBusinessByUserIdAsync(Guid userId);
        Task<List<UserBusiness>> GetEmployeesByBusinessIdAsync(Guid businessId);
        Task<List<UserBusiness>> GetEmployeesByServiceIdAsync(Guid serviceId, Guid businessId);
    }
}
