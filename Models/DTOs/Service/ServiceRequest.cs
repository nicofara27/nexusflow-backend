namespace NexusFlow.Models.DTOs.Service
{
    public class ServiceRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Price { get; set; }
        public int Duration {  get; set; }
        public Guid? ServiceCategoryId { get; set; }
    }
}
