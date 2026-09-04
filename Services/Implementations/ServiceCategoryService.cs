using NexusFlow.Models.DTOs.ServiceCategory;
using NexusFlow.Models.Entities;
using NexusFlow.Models.Enums;
using NexusFlow.Repositories.Interfaces;
using NexusFlow.Services.Interfaces;

namespace NexusFlow.Services.Implementations
{
    public class ServiceCategoryService : IServiceCategoryService
    {
        private readonly IServiceCategoryRepository _serviceCategoryRepository;
        private readonly IUserBusinessRepository _userBusinessRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ServiceCategoryService(
            IServiceCategoryRepository serviceCategoryRepository, 
            IUserBusinessRepository userBusinessRepository, 
            IUnitOfWork unitOfWork)
        {
            _serviceCategoryRepository = serviceCategoryRepository;
            _userBusinessRepository = userBusinessRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<List<ServiceCategoryResponse>> GetAllAsync(Guid adminId)
        {
            var admin = await GetAdminAsync(adminId);

            var categories = await _serviceCategoryRepository.GetByBusinessIdAsync(admin.BusinessId);

            return categories.Select(MapToResponse).ToList();
        }

        public async Task<ServiceCategoryResponse> CreateAsync(ServiceCategoryRequest dto, Guid adminId)
        {
            var admin = await GetAdminAsync(adminId);

            var name = dto.Name.Trim();

            if(string.IsNullOrWhiteSpace(name))
            {
                throw new Exception("El nombre de la categoría es obligatorio.");
            }

            var exists = await _serviceCategoryRepository.ExistsByNameAsync(admin.BusinessId, name);
            if(exists)
            {
                throw new Exception("Ya existe una categoría con ese nombre.");
            }

            var category = new ServiceCategory
            {
                Name = name,
                Order = dto.Order,
                BusinessId = admin.BusinessId
            };

            _serviceCategoryRepository.Add(category);
            await _unitOfWork.SaveChangesAsync();

            return MapToResponse(category);
        }

        public async Task<ServiceCategoryResponse> UpdateAsync(Guid categoryId, ServiceCategoryRequest dto, Guid adminId)
        {
            var admin = await GetAdminAsync(adminId);

            var category = await _serviceCategoryRepository.GetByIdAsync(categoryId);
            if (category == null || category.BusinessId != admin.BusinessId)
            {
                throw new Exception("La categoría de servicio no existe.");
            }

            var name = dto.Name.Trim();
            if(string.IsNullOrWhiteSpace(name))
            {
                throw new Exception("El nombre de la categoría es obligatorio.");
            }

            var exists = await _serviceCategoryRepository.ExistsByNameAsync(admin.BusinessId, name, categoryId);
            if(exists)
            {
                throw new Exception("Ya existe una categoría con ese nombre.");
            }

            category.Name = name;
            category.Order = dto.Order;

            await _unitOfWork.SaveChangesAsync();

            return MapToResponse(category);
        }

        public async Task DeleteAsync(Guid categoryId, Guid adminId)
        {
            var admin = await GetAdminAsync(adminId);

            var category = await _serviceCategoryRepository
                .GetByIdAsync(categoryId);

            if (category is null || category.BusinessId != admin.BusinessId)
            {
                throw new KeyNotFoundException(
                    "La categoría de servicio no existe.");
            }

            _serviceCategoryRepository.Delete(category);

            await _unitOfWork.SaveChangesAsync();
        }

        private async Task<UserBusiness> GetAdminAsync(Guid adminId)
        {
            var admin = await _userBusinessRepository.GetByUserIdAsync(adminId, BusinessRole.Admin);

            if (admin is null)
            {
                throw new Exception("El usuario no administra ningún negocio.");
            }

            return admin;
        }

        private static ServiceCategoryResponse MapToResponse(ServiceCategory category)
        {
            return new ServiceCategoryResponse
            {
                Id = category.Id,
                Name = category.Name,
                Order = category.Order,
            };
        }
    }
}
