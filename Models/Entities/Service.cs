namespace NexusFlow.Models.Entities
{
    public class Service
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public int Price { get; set; }
        public int Duration { get; set; }
        public bool IsActive { get; set; } = true;
        public Guid BusinessId { get; set; }
        public Business Business { get; set; } = null!;
        public ICollection<Appointment> Appointments { get; set; } = [];
        public ICollection<ServiceAssignment> ServiceAssignment { get; set; } = [];
    }
}
