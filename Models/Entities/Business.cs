namespace NexusFlow.Models.Entities
{
    public class Business
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? About { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? AccentColor { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Guid BusinessCategoryId { get; set; }
        public BusinessCategory BusinessCategory { get; set; } = null!;
        public ICollection<BusinessImage> Images { get; set; } = [];
        public ICollection<BusinessSchedule> Schedules { get; set; } = [];
        public ICollection<Service> Services { get; set; } = [];
        public ICollection<ServiceCategory> ServiceCategories { get; set; } = [];
        public ICollection<UserBusiness> UserBusinesses { get; set; } = [];
    }
}
