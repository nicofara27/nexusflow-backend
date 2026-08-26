using NexusFlow.Models.DTOs.Business;
using NexusFlow.Models.Entities;
using NexusFlow.Models.Enums;
using NexusFlow.Repositories.Interfaces;
using NexusFlow.Services.Interfaces;

namespace NexusFlow.Services.Implementations
{
    public class BusinessService : IBusinessService
    {
        private readonly IUserRepository _userRepository;
        private readonly IBusinessRepository _businessRepository;
        private readonly IUserBusinessRepository _userBusinessRepository;
        private readonly IUnitOfWork _unitOfWork;

        public BusinessService(IUserRepository userRepository,
            IBusinessRepository businessRepository,
            IUserBusinessRepository userBusinessRepository,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _businessRepository = businessRepository;
            _userBusinessRepository = userBusinessRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<BusinessResponse> CreateBusinessAsync(BusinessRequest dto, Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null) throw new Exception("Usuario no encontrado");

            var business = new Business
            {
                Name = dto.Name,
                Address = dto.Address,
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


            return new BusinessResponse
            {
                Id = business.Id,
                Name = business.Name,
                Address = business.Address,
            };
        }

        public async Task<BusinessResponse> GetBusinessAsync(Guid userId)
        {
            var userBusiness = await _userBusinessRepository.GetByUserIdWithBusinessAsync(userId);

            if (userBusiness == null) throw new Exception("Negocio no encontrado.");

            return new BusinessResponse
            {
                Name = userBusiness.Business.Name,
                Address = userBusiness.Business.Address,
            };
        }

        //public async Task<BusinessResponse> GetBusinessAdminAsync(business)
        //{

        //}

        public async Task<BusinessResponse> UpdateBusinessAsync(Guid userId, BusinessRequest dto)
        {
            var business = await _userBusinessRepository.GetBusinessByUserIdAsync(userId);

            if (business == null) throw new Exception("Negocio no encontrado.");

            business.Name = dto.Name;
            business.Address = dto.Address;

            await _unitOfWork.SaveChangesAsync();

            return new BusinessResponse
            {
                Name = business.Name,
                Address = business.Address,
            };
        }
    }
}
