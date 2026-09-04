namespace NexusFlow.Models.Entities
{
    public class ServiceCategory
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Order { get; set; }
        public Guid BusinessId { get; set; }
        public Business Business { get; set; } = null!;
        public ICollection<Service> Services { get; set; } = [];
    }
}
