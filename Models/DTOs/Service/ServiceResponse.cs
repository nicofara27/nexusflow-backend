namespace NexusFlow.Models.DTOs.Service
{
    public class ServiceResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Price { get; set; }
        public int Duration { get; set; }
    }
}
