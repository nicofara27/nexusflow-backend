using NexusFlow.Models.Entities;

namespace NexusFlow.Repositories.Interfaces
{
    public interface IServiceAssignmentRepository
    {
        Task DeleteByUserBusinessIdAsync(Guid userBusinessId);
        Task AddRangeAsync(List<ServiceAssignment> assignments);
    }
}
