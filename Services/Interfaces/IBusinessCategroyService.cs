using NexusFlow.Models.DTOs.BusinessCategory;

namespace NexusFlow.Services.Interfaces
{
    public interface IBusinessCategroyService
    {
        Task<List<BusinessCategoryResponse>> GetAllAsync();
    }
}
