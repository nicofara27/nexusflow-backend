namespace NexusFlow.Models.DTOs.ServiceCategory
{
    public class ServiceCategoryResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Order { get; set; }
    }
}
