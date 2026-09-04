using NexusFlow.Models.Entities;

namespace NexusFlow.Repositories.Interfaces
{
    public interface IBusinessCategoryRepository
    {
        Task<BusinessCategory?> GetByIdAsync(Guid id);
        Task<List<BusinessCategory>> GetAllAsync();
    }
}
