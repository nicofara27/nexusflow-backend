using NexusFlow.Models.DTOs.BusinessSchedule;
using NexusFlow.Models.DTOs.Service;

namespace NexusFlow.Models.DTOs.Business
{
    public class BusinessPublicDetailsResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? About { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? AccentColor { get; set; }
        public Guid BusinessCategoryId { get; set; }
        public string BusinessCategoryName { get; set; } = string.Empty;
        public string BusinessCategorySlug { get; set; } = string.Empty;
        public List<BusinessImagePublicResponse> Images { get; set; } = [];
        public List<BusinessScheduleResponse> Schedules { get; set; } = [];
        public List<ServiceCategoryPublicResponse> ServiceCategories { get; set; } = [];
        public List<ServiceResponse> Services { get; set; } = [];
        public List<EmployeePublicResponse> Employees { get; set; } = [];
    }
}
