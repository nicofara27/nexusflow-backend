namespace NexusFlow.Models.Entities
{
    public class ServiceAssignment
    {
        public Guid Id { get; set; }
        public Guid UserBusinessId { get; set; }
        public UserBusiness UserBusiness { get; set; } = null!;
        public Guid ServiceId { get; set; }
        public Service Service { get; set; } = null!;
    }
}
