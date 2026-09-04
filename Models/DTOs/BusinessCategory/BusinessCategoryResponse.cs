namespace NexusFlow.Models.DTOs.BusinessCategory
{
    public class BusinessCategoryResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
    }
}
