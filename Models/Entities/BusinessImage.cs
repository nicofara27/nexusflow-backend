using NexusFlow.Models.Entities;

public class BusinessImage
{
    public Guid Id { get; set; }
    public Guid BusinessId { get; set; }
    public Business Business { get; set; } = null!;
    public string StorageKey { get; set; } = string.Empty;
    public int Order { get; set; }
}