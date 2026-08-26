using Microsoft.EntityFrameworkCore;
using NexusFlow.Data;
using NexusFlow.Models.DTOs.Service;
using NexusFlow.Models.Entities;
using NexusFlow.Models.Enums;
using NexusFlow.Repositories.Implementations;
using NexusFlow.Repositories.Interfaces;
using NexusFlow.Services.Interfaces;

namespace NexusFlow.Services.Implementations
{
    public class ServicesService : IServicesService
    {
        private readonly IUserBusinessRepository _userBusinessRepository;
        private readonly IServiceRepository _serviceRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ServicesService(IUserBusinessRepository userBusinessRepository,
            IServiceRepository serviceRepository,
            IUnitOfWork unitOfWork)
        {
            _userBusinessRepository = userBusinessRepository;
            _serviceRepository = serviceRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ServiceResponse> CreateServiceAsync(ServiceRequest dto, Guid userId)
        {
            var userBusiness = await _userBusinessRepository.GetByUserIdAsync(userId, BusinessRole.Admin);
            if (userBusiness == null) throw new Exception("No tienes permisos para crear servicios");

            var service = new Service
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Duration = dto.Duration,
                BusinessId = userBusiness.BusinessId,
            };

            _serviceRepository.Add(service);
            await _unitOfWork.SaveChangesAsync();

            return new ServiceResponse
            {
                Id = service.Id,
                Name = service.Name,
            };
        }

        public async Task<List<ServiceResponse>> GetServicesByBusinessIdAsync(Guid businessId)
        {
            return await _serviceRepository.GetByBusinessIdAsync(businessId); ;
        }
        public async Task<ServiceResponse> GetServiceAsync(Guid serviceId, Guid userId)
        {
            var service = await _serviceRepository.GetByServiceIdAsync(serviceId);
            if (service == null) throw new Exception("No se encontró el servicio.");

            var isAdmin = await _userBusinessRepository.IsAdminAsync(userId, service.BusinessId);
            if (!isAdmin) throw new Exception("No tiene permiso para modificar este servicio.");

            return new ServiceResponse
            {
                Id = serviceId,
                Name = service.Name,
                Description = service.Description,
                Price = service.Price,
                Duration = service.Duration
            };
        }

        public async Task<ServiceResponse> UpdateServiceAsync(Guid serviceId, UpdateServiceRequest dto, Guid userId)
        {
            var service = await _serviceRepository.GetByServiceIdAsync(serviceId);
            if (service == null) throw new Exception("No se encontró el servicio.");
            var isAdmin = await _userBusinessRepository.IsAdminAsync(userId, service.BusinessId);
            if (!isAdmin) throw new Exception("No tiene permiso para modificar este servicio.");

            service.Name = dto.Name;
            service.Description = dto.Description;
            service.Price = dto.Price;
            service.Duration = dto.Duration;
            service.IsActive = dto.IsActive;

            await _unitOfWork.SaveChangesAsync();

            return new ServiceResponse
            {
                Id = service.Id,
                Name = service.Name,
                Description = service.Description,
                Price = service.Price,
                Duration = service.Duration
            };
        }
    }
}
