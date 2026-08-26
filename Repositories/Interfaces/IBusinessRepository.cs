using NexusFlow.Models.DTOs;
using NexusFlow.Models.Entities;

namespace NexusFlow.Repositories.Interfaces
{
    public interface IBusinessRepository
    {
        void Add(Business business);
        Task<Business?> GetByIdAsync(Guid businessId);
    }
}
