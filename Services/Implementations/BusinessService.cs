using NexusFlow.Models.DTOs.Business;
using NexusFlow.Models.DTOs.BusinessSchedule;
using NexusFlow.Models.DTOs.Service;
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


            return MapToResponse(business, category.Name);

        }

        public async Task<BusinessResponse> GetBusinessAsync(Guid userId)
        {
            var userBusiness = await _userBusinessRepository.GetByUserIdWithBusinessAsync(userId);

            if (userBusiness == null) throw new Exception("Negocio no encontrado.");

            return MapToResponse(userBusiness.Business, userBusiness.Business.BusinessCategory.Name);

        }

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

            return MapToResponse(business, category.Name);
        }

        public async Task<List<BusinessPublicResponse>> GetAllPublicAsync()
        {
            var businesses = await _businessRepository.GetPublicBusinessesAsync();

            return businesses
            .Select(MapToPublicResponse)
            .ToList();
        }

        public async Task<BusinessPublicDetailsResponse> GetPublicByIdAsync(Guid businessId)
        {
            var business = await _businessRepository.GetPublicBusinessByIdAsync(businessId);
            if(business == null) throw new Exception("Negocio no encontrado.");

            return MapToPublicDetailsResponse(business);
        }

        private static BusinessResponse MapToResponse(Business business, string businessCategoryName)
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
                BusinessCategoryName = businessCategoryName
            };
        }

        private static BusinessPublicResponse MapToPublicResponse(Business business)
        {
            return new BusinessPublicResponse
            {
                Id = business.Id,
                Name = business.Name,
                Address = business.Address,
                BusinessCategoryId = business.BusinessCategoryId,
                BusinessCategoryName = business.BusinessCategory.Name,
                BusinessCategorySlug = business.BusinessCategory.Slug,
                MainImageUrl = business.Images
                    .OrderBy(image => image.Order)
                    .Select(image => image.StorageKey)
                    .FirstOrDefault()
            };
        }

        private static BusinessPublicDetailsResponse MapToPublicDetailsResponse(
            Business business)
        {
            var usesServiceCategories = business.ServiceCategories.Any();
            var reviews = business.Reviews
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ReviewResponse
                {
                    Id = r.Id,
                    Author = $"{r.Appointment.Client.FirstName} {r.Appointment.Client.LastName}",
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                })
                .ToList();

            return new BusinessPublicDetailsResponse
            {
                Id = business.Id,
                Name = business.Name,
                Address = business.Address,
                About = business.About,
                Latitude = business.Latitude,
                Longitude = business.Longitude,
                AccentColor = business.AccentColor,
                BusinessCategoryId = business.BusinessCategoryId,
                BusinessCategoryName = business.BusinessCategory.Name,
                BusinessCategorySlug = business.BusinessCategory.Slug,

                AverageRating = reviews.Count > 0
                    ? reviews.Average(r => r.Rating)
                    : 0,
                TotalReviews = reviews.Count,
                Reviews = reviews,

                Images = business.Images
                    .OrderBy(image => image.Order)
                    .Select(image => new BusinessImagePublicResponse
                    {
                        Id = image.Id,
                        Url = image.StorageKey,
                        Order = image.Order
                    })
                    .ToList(),

                Schedules = business.Schedules
                    .OrderBy(schedule => GetDayOrder(schedule.DayOfWeek))
                    .Select(schedule => new BusinessScheduleResponse
                    {
                        DayOfWeek = schedule.DayOfWeek,
                        StartTime = schedule.StartTime,
                        EndTime = schedule.EndTime
                    })
                    .ToList(),

                ServiceCategories = usesServiceCategories
                    ? business.ServiceCategories
                        .OrderBy(category => category.Order)
                        .Select(category => new ServiceCategoryPublicResponse
                        {
                            Id = category.Id,
                            Name = category.Name,
                            Order = category.Order,
                            Services = business.Services
                                .Where(service =>
                                    service.IsActive &&
                                    service.ServiceCategoryId == category.Id)
                                .Select(MapServiceToResponse)
                                .ToList()
                        })
                        .ToList()
                    : [],

                Services = !usesServiceCategories
                    ? business.Services
                        .Where(service => service.IsActive)
                        .Select(MapServiceToResponse)
                        .ToList()
                    : [],

                Employees = business.UserBusinesses
                    .Where(userBusiness =>
                        userBusiness.Role == BusinessRole.Employee &&
                        userBusiness.IsActive)
                    .Select(userBusiness => new EmployeePublicResponse
                    {
                        Id = userBusiness.Id,
                        FirstName = userBusiness.User.FirstName,
                        LastName = userBusiness.User.LastName
                    })
                    .ToList()
            };
        }

        private static ServiceResponse MapServiceToResponse(Service service)
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

        private static int GetDayOrder(DayOfWeek day)
        {
            return day == DayOfWeek.Sunday ? 7 : (int)day;
        }
    }
}
