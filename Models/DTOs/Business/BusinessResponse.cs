namespace NexusFlow.Models.DTOs.Business
{
    public class BusinessResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address {  get; set; } = string.Empty;
        public string? About { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? AccentColor { get; set; }
        public Guid BusinessCategoryId { get; set; }
        public string BusinessCategoryName { get; set; } = string.Empty;
    }
}
