namespace NexusFlow.Models.Entities
{
    public class Business
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public ICollection<UserBusiness> UserBusinesses { get; set; } = [];
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<Service> Services { get; set; } = [];

    }
}
