using NexusFlow.Models.DTOs.Business;

namespace NexusFlow.Services.Interfaces
{
    public interface IBusinessService
    {
        Task<BusinessResponse> CreateBusinessAsync(BusinessRequest dto, Guid userId);
        Task<BusinessResponse> GetBusinessAsync(Guid userId);
        Task<BusinessResponse> UpdateBusinessAsync(Guid userId, BusinessRequest dto);
    }
}
