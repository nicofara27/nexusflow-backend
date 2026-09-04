using NexusFlow.Models.DTOs.Business;
using NexusFlow.Models.Entities;
using NexusFlow.Models.Enums;
using NexusFlow.Repositories.Interfaces;
using NexusFlow.Services.Interfaces;

namespace NexusFlow.Services.Implementations
{
    public class BusinessService : IBusinessService
    {
        private readonly IBusinessRepository _businessRepository;
        private readonly IBusinessCategoryRepository _businessCategoryRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUserBusinessRepository _userBusinessRepository;
        private readonly IUnitOfWork _unitOfWork;

        public BusinessService(IBusinessRepository businessRepository, 
            IBusinessCategoryRepository businessCategoryRepository,
            IUserRepository userRepository,
            IUserBusinessRepository userBusinessRepository,
            IUnitOfWork unitOfWork)
        {
            _businessRepository = businessRepository;
            _businessCategoryRepository = businessCategoryRepository;
            _userRepository = userRepository;
            _userBusinessRepository = userBusinessRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<BusinessResponse> CreateBusinessAsync(BusinessRequest dto, Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null) throw new Exception("Usuario no encontrado");

            var category = await _businessCategoryRepository.GetByIdAsync(dto.BusinessCategoryId);

            if (category == null) throw new Exception("Categoría de negocio no encontrada");

            var business = new Business
            {
                Name = dto.Name,
                Address = dto.Address,
                About = dto.About,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                AccentColor = dto.AccentColor,
                BusinessCategoryId = dto.BusinessCategoryId,
                CreatedAt = DateTime.Now
            };

            _businessRepository.Add(business);
            await _unitOfWork.SaveChangesAsync();

            var userBusiness = new UserBusiness
            {
                UserId = userId,
                BusinessId = business.Id,
                Role = BusinessRole.Admin
            };

            _userBusinessRepository.Add(userBusiness);
            await _unitOfWork.SaveChangesAsync();


            return MapToResponse(business);

        }

        public async Task<BusinessResponse> GetBusinessAsync(Guid userId)
        {
            var userBusiness = await _userBusinessRepository.GetByUserIdWithBusinessAsync(userId);

            if (userBusiness == null) throw new Exception("Negocio no encontrado.");

            return MapToResponse(userBusiness.Business);

        }

        //public async Task<BusinessResponse> GetBusinessAdminAsync(business)
        //{

        //}

        public async Task<BusinessResponse> UpdateBusinessAsync(Guid userId, BusinessRequest dto)
        {
            var business = await _userBusinessRepository.GetBusinessByUserIdAsync(userId);
            if (business == null) throw new Exception("Negocio no encontrado.");

            var category = await _businessCategoryRepository.GetByIdAsync(dto.BusinessCategoryId);
            if (category == null) throw new Exception("Categoría de negocio no encontrada");

            if (dto.Latitude.HasValue != dto.Longitude.HasValue)
            {
                throw new Exception("La latitud y longitud deben proporcionarse juntas.");
            }

            business.Name = dto.Name;
            business.Address = dto.Address;
            business.About = dto.About;
            business.Latitude = dto.Latitude;
            business.Longitude = dto.Longitude;
            business.AccentColor = dto.AccentColor;
            business.BusinessCategoryId = dto.BusinessCategoryId;

            await _unitOfWork.SaveChangesAsync();

            return MapToResponse(business);
        }

        private static BusinessResponse MapToResponse(Business business)
        {
            return new BusinessResponse
            {
                Id = business.Id,
                Name = business.Name,
                Address = business.Address,
                About = business.About,
                Latitude = business.Latitude,
                Longitude = business.Longitude,
                AccentColor = business.AccentColor,
                BusinessCategoryId = business.BusinessCategoryId,
                BusinessCategoryName = business.BusinessCategory.Name
            };
        }
    }
}
