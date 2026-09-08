using NexusFlow.Models.Entities;

namespace NexusFlow.Repositories.Interfaces
{
    public interface IServiceRepository
    {
        void Add(Service service);
        Task<Service?> GetByServiceIdAsync(Guid seviceId);
        Task<List<Service>> GetByIdsAsync(List<Guid> serviceIds);
        Task<List<Service>> GetByBusinessIdAsync(Guid businessId);
        Task<Service?> GetByIdAndBusinessAsync(Guid serviceId, Guid businessId);
    }
}
