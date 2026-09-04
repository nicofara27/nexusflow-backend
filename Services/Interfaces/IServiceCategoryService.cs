using NexusFlow.Models.DTOs.ServiceCategory;

namespace NexusFlow.Services.Interfaces
{
    public interface IServiceCategoryService
    {
        Task<List<ServiceCategoryResponse>> GetAllAsync(Guid adminId);
        Task<ServiceCategoryResponse> CreateAsync(ServiceCategoryRequest dto, Guid adminId);
        Task<ServiceCategoryResponse> UpdateAsync(Guid serviceCategoryId, ServiceCategoryRequest dto, Guid adminId);
        Task DeleteAsync(Guid serviceCategoryId, Guid adminId);
    }
}
