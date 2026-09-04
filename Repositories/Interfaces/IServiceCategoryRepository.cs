using NexusFlow.Models.Entities;

namespace NexusFlow.Repositories.Interfaces
{
    public interface IServiceCategoryRepository
    {
        Task<List<ServiceCategory>> GetByBusinessIdAsync(Guid businessId);
        Task<ServiceCategory?> GetByIdAsync(Guid id);
        Task<bool> ExistsByNameAsync(Guid businesId, string name, Guid? excludeId =null);
        void Add(ServiceCategory serviceCategory);
        void Delete(ServiceCategory serviceCategory);
    }
}
