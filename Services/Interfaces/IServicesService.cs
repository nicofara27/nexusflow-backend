using NexusFlow.Models.DTOs.Service;

namespace NexusFlow.Services.Interfaces
{
    public interface IServicesService
    {
        Task<ServiceResponse> CreateServiceAsync(ServiceRequest dto, Guid userId);
        Task<List<ServiceResponse>> GetServicesByBusinessIdAsync(Guid businessId);
        Task<ServiceResponse> GetServiceAsync(Guid serviceId, Guid userId);
        Task<ServiceResponse> UpdateServiceAsync(Guid serviceId, UpdateServiceRequest dto, Guid userId);
    }
}
