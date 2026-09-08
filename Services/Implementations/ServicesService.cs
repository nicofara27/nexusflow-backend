using NexusFlow.Models.DTOs.Service;
using NexusFlow.Models.Entities;
using NexusFlow.Models.Enums;
using NexusFlow.Repositories.Interfaces;
using NexusFlow.Services.Interfaces;

namespace NexusFlow.Services.Implementations
{
    public class ServicesService : IServicesService
    {
        private readonly IUserBusinessRepository _userBusinessRepository;
        private readonly IServiceRepository _serviceRepository;
        private readonly IServiceCategoryRepository _serviceCategoryRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ServicesService(IUserBusinessRepository userBusinessRepository,
            IServiceRepository serviceRepository,
            IServiceCategoryRepository serviceCategoryRepository,
            IUnitOfWork unitOfWork)
        {
            _userBusinessRepository = userBusinessRepository;
            _serviceRepository = serviceRepository;
            _serviceCategoryRepository = serviceCategoryRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ServiceResponse> CreateServiceAsync(ServiceRequest dto, Guid userId)
        {
            var userBusiness = await _userBusinessRepository.GetByUserIdAsync(userId, BusinessRole.Admin);
            if (userBusiness == null) throw new Exception("No tienes permisos para crear servicios");

            await ValidateServiceCategoryAsync(dto.ServiceCategoryId, userBusiness.BusinessId);

            var service = new Service
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Duration = dto.Duration,
                BusinessId = userBusiness.BusinessId,
                ServiceCategoryId = dto.ServiceCategoryId
            };

            _serviceRepository.Add(service);
            await _unitOfWork.SaveChangesAsync();

            return MapToResponse(service);
        }

        public async Task<List<ServiceResponse>> GetServicesByBusinessIdAsync(Guid businessId)
        {
            var services = await _serviceRepository
                .GetByBusinessIdAsync(businessId);

            return services.Select(MapToResponse).ToList();
        }
        public async Task<ServiceResponse> GetServiceAsync(Guid serviceId, Guid userId)
        {
            var service = await _serviceRepository.GetByServiceIdAsync(serviceId);
            if (service == null) throw new Exception("No se encontró el servicio.");

            var isAdmin = await _userBusinessRepository.IsAdminAsync(userId, service.BusinessId);
            if (!isAdmin) throw new Exception("No tiene permiso para modificar este servicio.");

            return MapToResponse(service);
        }

        public async Task<ServiceResponse> UpdateServiceAsync(Guid serviceId, UpdateServiceRequest dto, Guid userId)
        {
            var service = await _serviceRepository.GetByServiceIdAsync(serviceId);
            if (service == null) throw new Exception("No se encontró el servicio.");
            var isAdmin = await _userBusinessRepository.IsAdminAsync(userId, service.BusinessId);
            if (!isAdmin) throw new Exception("No tiene permiso para modificar este servicio.");

            await ValidateServiceCategoryAsync(dto.ServiceCategoryId, service.BusinessId);

            service.Name = dto.Name;
            service.Description = dto.Description;
            service.Price = dto.Price;
            service.Duration = dto.Duration;
            service.IsActive = dto.IsActive;
            service.ServiceCategoryId = dto.ServiceCategoryId;

            await _unitOfWork.SaveChangesAsync();

            return MapToResponse(service);
        }
        private async Task ValidateServiceCategoryAsync(
            Guid? serviceCategoryId,
            Guid businessId)
        {
            if (!serviceCategoryId.HasValue)
            {
                return;
            }

            var category = await _serviceCategoryRepository
                .GetByIdAsync(serviceCategoryId.Value);

            if (category is null || category.BusinessId != businessId)
            {
                throw new KeyNotFoundException(
                    "La categoría de servicio no existe.");
            }
        }

        private static ServiceResponse MapToResponse(Service service)
        {
            return new ServiceResponse
            {
                Id = service.Id,
                Name = service.Name,
                Description = service.Description,
                Price = service.Price,
                Duration = service.Duration,
                ServiceCategoryId = service.ServiceCategoryId
            };
        }
    }
}
