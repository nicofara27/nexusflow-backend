namespace NexusFlow.Models.DTOs.Business
{
    public class BusinessPublicResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public Guid BusinessCategoryId { get; set; }
        public string BusinessCategoryName { get; set; } = string.Empty;
        public string BusinessCategorySlug { get; set; } = string.Empty;
        public string? MainImageUrl { get; set; }
    }
}
