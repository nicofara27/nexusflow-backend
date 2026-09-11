using NexusFlow.Models.DTOs.Service;

public class ServiceCategoryPublicResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }
    public List<ServiceResponse> Services { get; set; } = [];
}